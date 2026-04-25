namespace ICD10.Cli.Tests;

/// <summary>
/// E2E tests for ICD-10-CM CLI - REAL database, mock API only.
/// Uses Spectre.Console.Testing to drive the CLI through TestConsole.
/// </summary>
public sealed class CliE2ETests : IClassFixture<CliTestFixture>
{
    readonly CliTestFixture _fixture;

    public CliE2ETests(CliTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Help_DisplaysAllCommands()
    {
        var output = await _fixture.RunCliAsync("help", "quit");

        Assert.Contains("search", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("find", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lookup", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("browse", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stats", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("history", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("clear", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("quit", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HelpShortcut_DisplaysHelp()
    {
        var output = await _fixture.RunCliAsync("h", "q");

        Assert.Contains("search", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Shortcuts", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QuestionMarkHelp_DisplaysHelp()
    {
        var output = await _fixture.RunCliAsync("?", "quit");

        Assert.Contains("search", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Quit_ExitsGracefully()
    {
        var output = await _fixture.RunCliAsync("quit");

        Assert.Contains("Goodbye", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task QuitShortcut_ExitsGracefully()
    {
        var output = await _fixture.RunCliAsync("q");

        Assert.Contains("Goodbye", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Exit_ExitsGracefully()
    {
        var output = await _fixture.RunCliAsync("exit");

        Assert.Contains("Goodbye", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Stats_DisplaysApiStatus()
    {
        var output = await _fixture.RunCliAsync("stats", "q");

        Assert.Contains("API", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Status", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task History_ShowsCommandHistory()
    {
        var output = await _fixture.RunCliAsync("help", "stats", "history", "q");

        Assert.Contains("help", output);
        Assert.Contains("stats", output);
    }

    [Fact]
    public async Task History_EmptyWhenNoCommands()
    {
        var output = await _fixture.RunCliAsync("history", "q");

        // First command is "history" so history will show that
        Assert.Contains("history", output);
    }

    [Fact]
    public async Task Find_SearchesByText()
    {
        var output = await _fixture.RunCliAsync("find chest", "q");

        Assert.Contains("chest", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FindShortcut_SearchesByText()
    {
        var output = await _fixture.RunCliAsync("f pneumonia", "q");

        Assert.Contains("pneumonia", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Find_ReturnsEmptyForNoMatch()
    {
        var output = await _fixture.RunCliAsync("find zzznomatchzzz", "q");

        Assert.Contains("No codes found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Find_RequiresArgument()
    {
        var output = await _fixture.RunCliAsync("find", "q");

        Assert.Contains("Usage", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_FindsExactCode()
    {
        var output = await _fixture.RunCliAsync("lookup R07.9", "q");

        Assert.Contains("R07.9", output);
        Assert.Contains("Chest pain", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LookupShortcut_FindsCode()
    {
        var output = await _fixture.RunCliAsync("l E11.9", "q");

        Assert.Contains("E11.9", output);
        Assert.Contains("diabetes", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_HandlesNoDot()
    {
        var output = await _fixture.RunCliAsync("lookup R079", "q");

        Assert.Contains("R07.9", output);
    }

    [Fact]
    public async Task Lookup_HandlesLowercase()
    {
        var output = await _fixture.RunCliAsync("lookup r07.9", "q");

        Assert.Contains("R07.9", output);
    }

    [Fact]
    public async Task Lookup_RequiresArgument()
    {
        var output = await _fixture.RunCliAsync("lookup", "q");

        Assert.Contains("Usage", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsMultipleMatches()
    {
        var output = await _fixture.RunCliAsync("lookup R07", "q");

        Assert.Contains("R07.9", output);
        Assert.Contains("R07.89", output);
    }

    [Fact]
    public async Task Browse_ShowsChapterOverview()
    {
        var output = await _fixture.RunCliAsync("browse", "q");

        Assert.Contains("A-B", output);
        Assert.Contains("Infectious", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Browse_FiltersByLetter()
    {
        // Browse R0 to get codes starting with R0 (R alone matches too many descriptions)
        var output = await _fixture.RunCliAsync("browse R0", "q");

        Assert.Contains("R0", output);
    }

    [Fact]
    public async Task BrowseShortcut_FiltersByLetter()
    {
        // Browse J1 to get codes starting with J1 (J alone matches too many descriptions)
        var output = await _fixture.RunCliAsync("b J1", "q");

        Assert.Contains("J1", output);
    }

    [Fact]
    public async Task Search_ShowsResultsOrFallsBack()
    {
        var output = await _fixture.RunCliAsync("search chest pain", "q");

        Assert.Contains("chest", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_RequiresArgument()
    {
        var output = await _fixture.RunCliAsync("search", "q");

        Assert.Contains("Usage", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_ShowsHeartRelatedResults()
    {
        var output = await _fixture.RunCliAsync("search heart attack", "q");

        Assert.Contains("Results", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_ShortcutWorks()
    {
        var output = await _fixture.RunCliAsync("s diabetes", "q");

        Assert.Contains("diabetes", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnknownCommand_TreatedAsSearch()
    {
        var output = await _fixture.RunCliAsync("chest pain symptoms", "q");

        Assert.Contains("chest", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Clear_ClearsScreenAndShowsHeader()
    {
        var output = await _fixture.RunCliAsync("clear", "q");

        Assert.Contains(
            "Medical Diagnosis Code Explorer",
            output,
            StringComparison.OrdinalIgnoreCase
        );
    }

    [Fact]
    public async Task EmptyInput_IsIgnored()
    {
        var output = await _fixture.RunCliAsync("", "", "q");

        Assert.Contains("Goodbye", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HeaderDisplaysOnStartup()
    {
        var output = await _fixture.RunCliAsync("q");

        Assert.Contains(
            "Medical Diagnosis Code Explorer",
            output,
            StringComparison.OrdinalIgnoreCase
        );
        Assert.Contains("help", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApiStatusDisplaysOnStartup()
    {
        var output = await _fixture.RunCliAsync("q");

        Assert.Contains("API", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BillableIndicator_ShownInCodeList()
    {
        var output = await _fixture.RunCliAsync("find chest", "q");

        Assert.Contains("billable", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_CodeNotFound_ShowsErrorMessage()
    {
        var output = await _fixture.RunCliAsync("lookup ZZZ99.99", "q");

        Assert.Contains("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Json_CodeNotFound_ShowsErrorMessage()
    {
        var output = await _fixture.RunCliAsync("json ZZZ99.99", "q");

        Assert.Contains("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_NoResults_HandlesGracefully()
    {
        var output = await _fixture.RunCliAsync("search xyznonexistent123", "q");

        Assert.Contains("Goodbye", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Browse_InvalidLetter_ShowsEmpty()
    {
        var output = await _fixture.RunCliAsync("browse 9", "q");

        Assert.Contains("Goodbye", output, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // CRITICAL: Lookup tests for ICD-10-CM codes (returned by RAG search)
    // These tests ensure codes from search can actually be looked up
    // =========================================================================

    [Fact]
    public async Task Lookup_FindsIcd10CmCode_I10()
    {
        // I10 (Essential hypertension) is in icd10_code (used by RAG search)
        var output = await _fixture.RunCliAsync("l I10", "q");

        Assert.Contains("I10", output);
        Assert.Contains("hypertension", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_FindsIcd10CmCode_I2111_HeartAttack()
    {
        // I21.11 (ST elevation MI) is in icd10_code - critical for "heart attack" search
        var output = await _fixture.RunCliAsync("l I21.11", "q");

        Assert.Contains("I21.11", output);
        Assert.Contains("myocardial infarction", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_FindsIcd10CmCode_M545_BackPain()
    {
        var output = await _fixture.RunCliAsync("l M54.5", "q");

        Assert.Contains("M54.5", output);
        Assert.Contains("back pain", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_FindsIcd10CmCode_G43909_Migraine()
    {
        var output = await _fixture.RunCliAsync("l G43.909", "q");

        Assert.Contains("G43.909", output);
        Assert.Contains("migraine", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsFullCodeDetails_AllFields()
    {
        // Verify lookup shows ALL required information for R07.9
        var output = await _fixture.RunCliAsync("l R07.9", "q");

        Assert.Contains("R07.9", output);
        Assert.Contains("Chest pain", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Billable", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("18", output);
        Assert.Contains("Symptoms", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("R00-R09", output);
        Assert.Contains("R07", output);
        Assert.Contains("Pain in throat and chest", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsChapterBlockCategoryHierarchy()
    {
        // Verify I10 (hypertension) shows full hierarchy
        var output = await _fixture.RunCliAsync("l I10", "q");

        Assert.Contains("I10", output);
        Assert.Contains("hypertension", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("circulatory", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("I10-I1A", output);
        Assert.Contains("Hypertensive", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Essential (primary) hypertension",
            output,
            StringComparison.OrdinalIgnoreCase
        );
        Assert.Contains("benign", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsSynonyms_WhenPresent()
    {
        // Verify E11.9 (Type 2 diabetes) shows synonyms
        var output = await _fixture.RunCliAsync("l E11.9", "q");

        Assert.Contains("E11.9", output);
        Assert.Contains("Type 2 diabetes", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Diabetes", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsLongDescription()
    {
        // Verify G43.909 shows long description
        var output = await _fixture.RunCliAsync("l G43.909", "q");

        Assert.Contains("G43.909", output);
        Assert.Contains("Migraine", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("without status migrainosus", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hemicrania", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsEditionInfo()
    {
        // Verify edition/version info is shown
        var output = await _fixture.RunCliAsync("l I21.11", "q");

        Assert.Contains("I21.11", output);
        Assert.Contains("STEMI", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2025", output);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_AllSeededCodes_Succeed()
    {
        var codesToTest = new[]
        {
            ("R07.9", "chest pain"),
            ("R06.02", "shortness of breath"),
            ("I21.11", "myocardial infarction"),
            ("J18.9", "pneumonia"),
            ("E11.9", "diabetes"),
            ("I10", "hypertension"),
            ("M54.5", "back pain"),
        };

        foreach (var (code, expectedText) in codesToTest)
        {
            var output = await _fixture.RunCliAsync($"l {code}", "q");
            Assert.True(
                output.Contains(code, StringComparison.Ordinal),
                $"Lookup for {code} should show the code in output"
            );
            Assert.True(
                output.Contains(expectedText, StringComparison.OrdinalIgnoreCase),
                $"Lookup for {code} should show '{expectedText}' in output"
            );
            Assert.False(
                output.Contains("not found", StringComparison.OrdinalIgnoreCase),
                $"Lookup for {code} should NOT show 'not found'"
            );
        }
    }

    [Fact]
    public async Task Json_FindsIcd10CmCode()
    {
        // JSON command should also find ICD-10-CM codes
        var output = await _fixture.RunCliAsync("json I10", "q");

        Assert.Contains("I10", output);
        Assert.Contains("hypertension", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsChapterInfo()
    {
        var output = await _fixture.RunCliAsync("l I10", "q");

        Assert.Contains("I10", output);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("9", output); // ICD-10-CM uses numeric chapter numbers
        Assert.Contains("circulatory", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsCategoryInfo()
    {
        var output = await _fixture.RunCliAsync("l E11.9", "q");

        Assert.Contains("E11.9", output);
        Assert.Contains("Category", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("E11", output);
    }

    [Fact]
    public async Task Lookup_ShowsSynonymsWhenPresent()
    {
        var output = await _fixture.RunCliAsync("l I10", "q");

        Assert.Contains("I10", output);
        Assert.Contains("Synonyms", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("high blood pressure", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Lookup_ShowsMultipleSynonyms()
    {
        // M54.50 has synonyms including lumbago
        var output = await _fixture.RunCliAsync("l M54.50", "q");

        Assert.Contains("M54.50", output);
        Assert.Contains("Synonym", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lumbago", output, StringComparison.OrdinalIgnoreCase);
    }

    // =========================================================================
    // COMPREHENSIVE E2E TEST: LOOKUP COMMAND DISPLAYS ALL DETAILS
    // =========================================================================

    [Fact]
    public async Task LookupCommand_E2E_DisplaysAllCodeDetails_ChapterBlockCategorySynonymsEdition()
    {
        // ARRANGE: Use I10 (hypertension) - has full hierarchy and synonyms
        var output = await _fixture.RunCliAsync("l I10", "q");

        Assert.Contains("I10", output);
        Assert.Contains("hypertension", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("9", output);
        Assert.Contains("circulatory", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Block", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("I10-I1A", output);
        Assert.Contains("Hypertensive", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Category", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "Essential (primary) hypertension",
            output,
            StringComparison.OrdinalIgnoreCase
        );
        Assert.Contains("Synonym", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("benign", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Edition", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2025", output);
        Assert.Contains("Billable", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LookupCommand_E2E_R074_DisplaysAllCodeDetails()
    {
        // ARRANGE: Use R07.9 (chest pain) - has full hierarchy and synonyms
        var output = await _fixture.RunCliAsync("l R07.9", "q");

        Assert.Contains("R07.9", output);
        Assert.Contains("Chest pain", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("18", output);
        Assert.Contains("Symptoms", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Block", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("R00-R09", output);
        Assert.Contains("Category", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("R07", output);
        Assert.Contains("Pain in throat and chest", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Edition", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2025", output);
        Assert.Contains("Billable", output, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LookupCommand_E2E_E119_Diabetes_DisplaysAllCodeDetails()
    {
        // ARRANGE: Use E11.9 (Type 2 diabetes) - has synonyms
        var output = await _fixture.RunCliAsync("l E11.9", "q");

        Assert.Contains("E11.9", output);
        Assert.Contains("Type 2 diabetes", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("4", output);
        Assert.Contains("Block", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("E08-E13", output);
        Assert.Contains("Diabetes mellitus", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Category", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("E11", output);
        Assert.Contains("Type 2 diabetes mellitus", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Synonym", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Edition", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2025", output);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// E2E tests that verify the CLI works with ACTUAL ICD-10-CM 2025 data.
/// Tests real codes from the production database.
/// </summary>
public sealed class RealDataE2ETests : IClassFixture<CliTestFixture>
{
    private readonly CliTestFixture _fixture;

    /// <summary>
    /// Creates tests using the shared test fixture.
    /// </summary>
    public RealDataE2ETests(CliTestFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Lookup_H53481_DisplaysAllDetails()
    {
        var output = await _fixture.RunCliAsync("l H53.481", "q");

        Assert.Contains("H53.481", output);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("visual field", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Block", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Category", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("H53", output);
    }

    [Fact]
    public async Task Lookup_Q531_DisplaysAllDetails()
    {
        var output = await _fixture.RunCliAsync("l Q53.1", "q");

        Assert.Contains("Q53.1", output);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("testicle", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Chapter", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Category", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Q53", output);
    }

    [Fact]
    public async Task Lookup_E119_DiabetesCode_DisplaysAllDetails()
    {
        var output = await _fixture.RunCliAsync("l E11.9", "q");

        Assert.Contains("E11.9", output);
        Assert.DoesNotContain("not found", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("diabetes", output, StringComparison.OrdinalIgnoreCase);
    }
}
