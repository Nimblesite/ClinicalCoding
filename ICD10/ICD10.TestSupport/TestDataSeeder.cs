using System.Net.Http.Json;
using System.Text.Json;
using Generated;
using Nimblesite.Sql.Model;
using Npgsql;
using Outcome;

namespace ICD10.TestSupport;

/// <summary>
/// Seeds ICD-10 reference data into a PostgreSQL test database via the
/// generated DataProvider Insert extension methods.
/// </summary>
public static class TestDataSeeder
{
    private const string IcdEmbeddingModel = "MedEmbed-Small-v0.1";

    /// <summary>
    /// Seeds chapters, blocks, categories, codes, ACHI blocks and ACHI codes
    /// required by both API and Dashboard E2E tests.
    /// </summary>
    public static void Seed(NpgsqlConnection conn)
    {
        SeedChaptersAsync(conn).GetAwaiter().GetResult();
        SeedBlocksAsync(conn).GetAwaiter().GetResult();
        SeedCategoriesAsync(conn).GetAwaiter().GetResult();
        SeedCodesAsync(conn).GetAwaiter().GetResult();
        SeedAchiBlocksAsync(conn).GetAwaiter().GetResult();
        SeedAchiCodesAsync(conn).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Seeds embeddings by calling the embedding service at localhost:8000.
    /// If the service is unavailable, silently returns (search tests will fail via skip check).
    /// </summary>
    public static void SeedEmbeddings(NpgsqlConnection conn) =>
        SeedEmbeddingsAsync(conn).GetAwaiter().GetResult();

    private static async Task SeedEmbeddingsAsync(NpgsqlConnection conn)
    {
        var icdItems = new (string EmbId, string CodeId, string Text)[]
        {
            (
                "emb-a00-0",
                "code-a00-0",
                "ICD-10 A00.0: Cholera due to Vibrio cholerae 01, biovar cholerae"
            ),
            (
                "emb-e10-9",
                "code-e10-9",
                "ICD-10 E10.9: Type 1 diabetes mellitus without complications"
            ),
            (
                "emb-e11-9",
                "code-e11-9",
                "ICD-10 E11.9: Type 2 diabetes mellitus without complications"
            ),
            (
                "emb-i10",
                "code-i10",
                "ICD-10 I10: Essential (primary) hypertension, high blood pressure"
            ),
            (
                "emb-i21-0",
                "code-i21-0",
                "ICD-10 I21.0: ST elevation myocardial infarction, heart attack, anterior wall"
            ),
            (
                "emb-i21-11",
                "code-i21-11",
                "ICD-10 I21.11: ST elevation myocardial infarction, heart attack, right coronary artery"
            ),
            (
                "emb-i21-4",
                "code-i21-4",
                "ICD-10 I21.4: Non-ST elevation myocardial infarction, NSTEMI, heart attack"
            ),
            (
                "emb-j06-9",
                "code-j06-9",
                "ICD-10 J06.9: Acute upper respiratory infection, unspecified"
            ),
            (
                "emb-r06-00",
                "code-r06-00",
                "ICD-10 R06.00: Dyspnea, shortness of breath, breathing difficulty"
            ),
            ("emb-r07-9", "code-r07-9", "ICD-10 R07.9: Chest pain, unspecified, thoracic pain"),
            ("emb-r07-89", "code-r07-89", "ICD-10 R07.89: Other chest pain, thoracic pain"),
            (
                "emb-a00-1",
                "code-a00-1",
                "ICD-10 A00.1: Cholera due to Vibrio cholerae 01, biovar eltor"
            ),
            ("emb-m54-5", "code-m54-5", "ICD-10 M54.5: Low back pain, lumbago, dorsalgia"),
            (
                "emb-s72-00",
                "code-s72-00",
                "ICD-10 S72.00: Fracture of unspecified part of neck of femur, hip fracture"
            ),
        };

        var achiItems = new (string EmbId, string CodeId, string Text)[]
        {
            (
                "emb-achi-38497",
                "achi-38497-00",
                "ACHI 38497-00: Coronary angiography, heart catheterization"
            ),
            (
                "emb-achi-38503",
                "achi-38503-00",
                "ACHI 38503-00: Percutaneous insertion of coronary artery stent, heart procedure"
            ),
            (
                "emb-achi-90661",
                "achi-90661-00",
                "ACHI 90661-00: Appendicectomy, appendix removal surgery"
            ),
            (
                "emb-achi-30571",
                "achi-30571-00",
                "ACHI 30571-00: Cholecystectomy, gallbladder removal surgery"
            ),
        };

        List<List<float>>? embeddings;
        try
        {
            embeddings = await FetchEmbeddingsAsync(
                icdItems.Select(t => t.Text).Concat(achiItems.Select(t => t.Text)).ToList()
            );
        }
        catch (HttpRequestException)
        {
            return;
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (embeddings is null || embeddings.Count != icdItems.Length + achiItems.Length)
            return;

        for (var i = 0; i < icdItems.Length; i++)
        {
            var (embId, codeId, _) = icdItems[i];
            EnsureInserted(
                await conn.Inserticd10_code_embeddingAsync(
                    Id: embId,
                    CodeId: codeId,
                    Embedding: SerializeVector(embeddings[i]),
                    EmbeddingModel: IcdEmbeddingModel,
                    LastUpdated: null
                ),
                "icd10_code_embedding",
                embId
            );
        }

        for (var i = 0; i < achiItems.Length; i++)
        {
            var (embId, codeId, _) = achiItems[i];
            EnsureInserted(
                await conn.Insertachi_code_embeddingAsync(
                    Id: embId,
                    CodeId: codeId,
                    Embedding: SerializeVector(embeddings[icdItems.Length + i]),
                    EmbeddingModel: IcdEmbeddingModel,
                    LastUpdated: null
                ),
                "achi_code_embedding",
                embId
            );
        }
    }

    private static async Task<List<List<float>>?> FetchEmbeddingsAsync(List<string> texts)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        var healthCheck = await client.GetAsync(new Uri("http://localhost:8000/health"));
        if (!healthCheck.IsSuccessStatusCode)
            return null;

        var batchResponse = await client.PostAsJsonAsync(
            new Uri("http://localhost:8000/embed/batch"),
            new { texts }
        );

        if (!batchResponse.IsSuccessStatusCode)
            return null;

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var batchResult = await batchResponse.Content.ReadFromJsonAsync<BatchEmbeddingResponse>(
            jsonOptions
        );
        return batchResult?.Embeddings;
    }

    private static string SerializeVector(List<float> values) =>
        "["
        + string.Join(
            ",",
            values.Select(f => f.ToString(System.Globalization.CultureInfo.InvariantCulture))
        )
        + "]";

    private sealed record BatchEmbeddingResponse(
        List<List<float>> Embeddings,
        string Model,
        int Dimensions,
        int Count
    );

    private static async Task SeedChaptersAsync(NpgsqlConnection conn)
    {
        var chapters = new (string Id, string Number, string Title, string Start, string End)[]
        {
            ("ch-01", "1", "Certain infectious and parasitic diseases", "A00", "B99"),
            ("ch-02", "2", "Neoplasms", "C00", "D49"),
            ("ch-03", "3", "Diseases of the blood and blood-forming organs", "D50", "D89"),
            ("ch-04", "4", "Endocrine, nutritional and metabolic diseases", "E00", "E89"),
            ("ch-05", "5", "Mental, behavioral and neurodevelopmental disorders", "F01", "F99"),
            ("ch-06", "6", "Diseases of the nervous system", "G00", "G99"),
            ("ch-07", "7", "Diseases of the eye and adnexa", "H00", "H59"),
            ("ch-08", "8", "Diseases of the ear and mastoid process", "H60", "H95"),
            ("ch-09", "9", "Diseases of the circulatory system", "I00", "I99"),
            ("ch-10", "10", "Diseases of the respiratory system", "J00", "J99"),
            ("ch-11", "11", "Diseases of the digestive system", "K00", "K95"),
            ("ch-12", "12", "Diseases of the skin and subcutaneous tissue", "L00", "L99"),
            (
                "ch-13",
                "13",
                "Diseases of the musculoskeletal system and connective tissue",
                "M00",
                "M99"
            ),
            ("ch-14", "14", "Diseases of the genitourinary system", "N00", "N99"),
            ("ch-15", "15", "Pregnancy, childbirth and the puerperium", "O00", "O9A"),
            ("ch-16", "16", "Certain conditions originating in the perinatal period", "P00", "P96"),
            ("ch-17", "17", "Congenital malformations and chromosomal abnormalities", "Q00", "Q99"),
            (
                "ch-18",
                "18",
                "Symptoms, signs and abnormal clinical and laboratory findings",
                "R00",
                "R99"
            ),
            (
                "ch-19",
                "19",
                "Injury, poisoning and certain other consequences of external causes",
                "S00",
                "T88"
            ),
            ("ch-20", "20", "External causes of morbidity", "V00", "Y99"),
            (
                "ch-21",
                "21",
                "Factors influencing health status and contact with health services",
                "Z00",
                "Z99"
            ),
        };

        foreach (var (id, number, title, start, end) in chapters)
        {
            EnsureInserted(
                await conn.Inserticd10_chapterAsync(
                    Id: id,
                    ChapterNumber: number,
                    Title: title,
                    CodeRangeStart: start,
                    CodeRangeEnd: end,
                    LastUpdated: null,
                    VersionId: null
                ),
                "icd10_chapter",
                id
            );
        }
    }

    private static async Task SeedBlocksAsync(NpgsqlConnection conn)
    {
        var blocks = new (
            string Id,
            string ChapterId,
            string BlockCode,
            string Title,
            string Start,
            string End
        )[]
        {
            ("blk-a00-a09", "ch-01", "A00-A09", "Intestinal infectious diseases", "A00", "A09"),
            ("blk-e08-e13", "ch-04", "E08-E13", "Diabetes mellitus", "E08", "E13"),
            ("blk-g40-g47", "ch-06", "G40-G47", "Episodic and paroxysmal disorders", "G40", "G47"),
            ("blk-h53-h54", "ch-07", "H53-H54", "Visual disturbances and blindness", "H53", "H54"),
            ("blk-i10-i1a", "ch-09", "I10-I1A", "Hypertensive diseases", "I10", "I1A"),
            ("blk-i20-i25", "ch-09", "I20-I25", "Ischaemic heart diseases", "I20", "I25"),
            ("blk-j00-j06", "ch-10", "J00-J06", "Acute upper respiratory infections", "J00", "J06"),
            (
                "blk-r00-r09",
                "ch-18",
                "R00-R09",
                "Symptoms and signs involving the circulatory and respiratory systems",
                "R00",
                "R09"
            ),
            ("blk-j09-j18", "ch-10", "J09-J18", "Influenza and pneumonia", "J09", "J18"),
            ("blk-r50-r69", "ch-18", "R50-R69", "General symptoms and signs", "R50", "R69"),
            ("blk-m50-m54", "ch-13", "M50-M54", "Other dorsopathies", "M50", "M54"),
            (
                "blk-q50-q56",
                "ch-17",
                "Q50-Q56",
                "Congenital malformations of genital organs",
                "Q50",
                "Q56"
            ),
            ("blk-s70-s79", "ch-19", "S70-S79", "Injuries to the hip and thigh", "S70", "S79"),
        };

        foreach (var (id, chapterId, blockCode, title, start, end) in blocks)
        {
            EnsureInserted(
                await conn.Inserticd10_blockAsync(
                    Id: id,
                    ChapterId: chapterId,
                    BlockCode: blockCode,
                    Title: title,
                    CodeRangeStart: start,
                    CodeRangeEnd: end,
                    LastUpdated: null,
                    VersionId: null
                ),
                "icd10_block",
                id
            );
        }
    }

    private static async Task SeedCategoriesAsync(NpgsqlConnection conn)
    {
        var categories = new (string Id, string BlockId, string CategoryCode, string Title)[]
        {
            ("cat-a00", "blk-a00-a09", "A00", "Cholera"),
            ("cat-e10", "blk-e08-e13", "E10", "Type 1 diabetes mellitus"),
            ("cat-e11", "blk-e08-e13", "E11", "Type 2 diabetes mellitus"),
            ("cat-g43", "blk-g40-g47", "G43", "Migraine"),
            ("cat-h53", "blk-h53-h54", "H53", "Visual disturbances"),
            ("cat-i10", "blk-i10-i1a", "I10", "Essential (primary) hypertension"),
            ("cat-i21", "blk-i20-i25", "I21", "Acute myocardial infarction"),
            (
                "cat-j06",
                "blk-j00-j06",
                "J06",
                "Acute upper respiratory infections of multiple and unspecified sites"
            ),
            ("cat-r06", "blk-r00-r09", "R06", "Abnormalities of breathing"),
            ("cat-r07", "blk-r00-r09", "R07", "Pain in throat and chest"),
            ("cat-j18", "blk-j09-j18", "J18", "Pneumonia, unspecified organism"),
            ("cat-r10", "blk-r00-r09", "R10", "Abdominal and pelvic pain"),
            ("cat-m54", "blk-m50-m54", "M54", "Dorsalgia"),
            ("cat-q53", "blk-q50-q56", "Q53", "Undescended and ectopic testicle"),
            ("cat-s72", "blk-s70-s79", "S72", "Fracture of femur"),
        };

        foreach (var (id, blockId, categoryCode, title) in categories)
        {
            EnsureInserted(
                await conn.Inserticd10_categoryAsync(
                    Id: id,
                    BlockId: blockId,
                    CategoryCode: categoryCode,
                    Title: title,
                    LastUpdated: null,
                    VersionId: null
                ),
                "icd10_category",
                id
            );
        }
    }

    private static async Task SeedCodesAsync(NpgsqlConnection conn)
    {
        var codes = new (
            string Id,
            string CategoryId,
            string Code,
            string Short,
            string Long,
            string Synonyms
        )[]
        {
            (
                "code-a00-0",
                "cat-a00",
                "A00.0",
                "Cholera due to Vibrio cholerae 01, biovar cholerae",
                "Cholera due to Vibrio cholerae 01, biovar cholerae",
                ""
            ),
            (
                "code-e10-9",
                "cat-e10",
                "E10.9",
                "Type 1 diabetes mellitus without complications",
                "Type 1 diabetes mellitus without complications",
                "juvenile diabetes"
            ),
            (
                "code-e11-9",
                "cat-e11",
                "E11.9",
                "Type 2 diabetes mellitus without complications",
                "Type 2 diabetes mellitus without complications",
                "adult-onset diabetes; non-insulin-dependent diabetes"
            ),
            (
                "code-e11-0",
                "cat-e11",
                "E11.0",
                "Type 2 diabetes mellitus with hyperosmolarity",
                "Type 2 diabetes mellitus with hyperosmolarity",
                ""
            ),
            (
                "code-e11-21",
                "cat-e11",
                "E11.21",
                "Type 2 diabetes mellitus with diabetic nephropathy",
                "Type 2 diabetes mellitus with diabetic nephropathy",
                "type 2 diabetes with kidney complications"
            ),
            (
                "code-e11-65",
                "cat-e11",
                "E11.65",
                "Type 2 diabetes mellitus with hyperglycemia",
                "Type 2 diabetes mellitus with hyperglycemia",
                ""
            ),
            (
                "code-g43-909",
                "cat-g43",
                "G43.909",
                "Migraine, unspecified, not intractable",
                "Migraine, unspecified, not intractable, without status migrainosus",
                "Hemicrania; sick headache"
            ),
            (
                "code-h53-481",
                "cat-h53",
                "H53.481",
                "Generalized contraction of visual field, left eye",
                "Generalized contraction of visual field, left eye",
                ""
            ),
            (
                "code-i10",
                "cat-i10",
                "I10",
                "Essential (primary) hypertension",
                "Essential (primary) hypertension",
                "benign hypertension; high blood pressure"
            ),
            (
                "code-i21-0",
                "cat-i21",
                "I21.0",
                "Acute transmural myocardial infarction of anterior wall",
                "ST elevation (STEMI) myocardial infarction involving left main coronary artery",
                ""
            ),
            (
                "code-i21-11",
                "cat-i21",
                "I21.11",
                "ST elevation (STEMI) myocardial infarction involving right coronary artery",
                "ST elevation (STEMI) myocardial infarction involving right coronary artery",
                ""
            ),
            (
                "code-i21-4",
                "cat-i21",
                "I21.4",
                "Acute subendocardial myocardial infarction",
                "Non-ST elevation (NSTEMI) myocardial infarction",
                ""
            ),
            (
                "code-j06-9",
                "cat-j06",
                "J06.9",
                "Acute upper respiratory infection, unspecified",
                "Acute upper respiratory infection, unspecified",
                ""
            ),
            (
                "code-j18-9",
                "cat-j18",
                "J18.9",
                "Pneumonia, unspecified organism",
                "Pneumonia, unspecified organism",
                ""
            ),
            (
                "code-r06-00",
                "cat-r06",
                "R06.00",
                "Dyspnea, unspecified",
                "Dyspnea, unspecified",
                ""
            ),
            ("code-r06-02", "cat-r06", "R06.02", "Shortness of breath", "Shortness of breath", ""),
            (
                "code-r07-9",
                "cat-r07",
                "R07.9",
                "Chest pain, unspecified",
                "Chest pain, unspecified",
                ""
            ),
            ("code-r07-89", "cat-r07", "R07.89", "Other chest pain", "Other chest pain", ""),
            (
                "code-a00-1",
                "cat-a00",
                "A00.1",
                "Cholera due to Vibrio cholerae 01, biovar eltor",
                "Cholera due to Vibrio cholerae 01, biovar eltor",
                ""
            ),
            ("code-m54-5", "cat-m54", "M54.5", "Low back pain", "Low back pain", ""),
            (
                "code-m54-50",
                "cat-m54",
                "M54.50",
                "Low back pain, unspecified",
                "Low back pain, unspecified",
                "lumbago; lumbar pain"
            ),
            (
                "code-q53-1",
                "cat-q53",
                "Q53.1",
                "Undescended testicle, unilateral",
                "Undescended testicle, unilateral",
                ""
            ),
            (
                "code-s72-00",
                "cat-s72",
                "S72.00",
                "Fracture of neck of femur, closed",
                "Fracture of unspecified part of neck of femur, closed",
                ""
            ),
        };

        foreach (var (id, categoryId, code, shortDesc, longDesc, synonyms) in codes)
        {
            EnsureInserted(
                await conn.Inserticd10_codeAsync(
                    Id: id,
                    CategoryId: categoryId,
                    Code: code,
                    ShortDescription: shortDesc,
                    LongDescription: longDesc,
                    InclusionTerms: "",
                    ExclusionTerms: "",
                    CodeAlso: "",
                    CodeFirst: "",
                    Synonyms: synonyms,
                    Billable: 1,
                    EffectiveFrom: "2025-07-01",
                    EffectiveTo: "",
                    Edition: "2025",
                    LastUpdated: null,
                    VersionId: null
                ),
                "icd10_code",
                id
            );
        }
    }

    private static async Task SeedAchiBlocksAsync(NpgsqlConnection conn)
    {
        var blocks = new (string Id, string BlockNumber, string Title, string Start, string End)[]
        {
            ("achi-blk-1", "1820", "Procedures on heart", "38497-00", "38503-00"),
            ("achi-blk-2", "0926", "Procedures on appendix", "90661-00", "90661-00"),
            (
                "achi-blk-3",
                "0965",
                "Procedures on gallbladder and biliary tract",
                "30571-00",
                "30575-00"
            ),
        };

        foreach (var (id, number, title, start, end) in blocks)
        {
            EnsureInserted(
                await conn.Insertachi_blockAsync(
                    Id: id,
                    BlockNumber: number,
                    Title: title,
                    CodeRangeStart: start,
                    CodeRangeEnd: end,
                    LastUpdated: null,
                    VersionId: null
                ),
                "achi_block",
                id
            );
        }
    }

    private static async Task SeedAchiCodesAsync(NpgsqlConnection conn)
    {
        var codes = new (string Id, string BlockId, string Code, string Short, string Long)[]
        {
            (
                "achi-38497-00",
                "achi-blk-1",
                "38497-00",
                "Coronary angiography",
                "Coronary angiography"
            ),
            (
                "achi-38503-00",
                "achi-blk-1",
                "38503-00",
                "Percutaneous insertion of coronary artery stent",
                "Percutaneous insertion of coronary artery stent"
            ),
            ("achi-90661-00", "achi-blk-2", "90661-00", "Appendicectomy", "Appendicectomy"),
            ("achi-30571-00", "achi-blk-3", "30571-00", "Cholecystectomy", "Cholecystectomy"),
        };

        foreach (var (id, blockId, code, shortDesc, longDesc) in codes)
        {
            EnsureInserted(
                await conn.Insertachi_codeAsync(
                    Id: id,
                    BlockId: blockId,
                    Code: code,
                    ShortDescription: shortDesc,
                    LongDescription: longDesc,
                    Billable: 1,
                    EffectiveFrom: "2025-07-01",
                    EffectiveTo: "",
                    Edition: "13",
                    LastUpdated: null,
                    VersionId: null
                ),
                "achi_code",
                id
            );
        }
    }

    private static void EnsureInserted(Result<Guid?, SqlError> result, string table, string id)
    {
        if (result is Result<Guid?, SqlError>.Error<Guid?, SqlError> err)
        {
            throw new InvalidOperationException(
                $"Insert into {table} for id '{id}' failed: {err.Value.Message}"
            );
        }
    }
}
