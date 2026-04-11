using System;
using System.Threading.Tasks;
using Dashboard.Models;
using H5;
using static H5.Core.dom;

namespace Dashboard.Api
{
    /// <summary>
    /// HTTP API client for Clinical and Scheduling microservices.
    /// </summary>
    public static class ApiClient
    {
        private static string _clinicalBaseUrl = "http://localhost:5080";
        private static string _schedulingBaseUrl = "http://localhost:5001";
        private static string _icd10BaseUrl = "http://localhost:5090";
        private static string _gatekeeperBaseUrl = "http://localhost:5002";

        // All microservices share a single token minted by Gatekeeper.
        private static string Token => Auth.GetToken() ?? "";

        /// <summary>Gatekeeper base URL for auth calls.</summary>
        public static string GatekeeperBaseUrl => _gatekeeperBaseUrl;

        /// <summary>
        /// Sets the base URLs for the microservices.
        /// </summary>
        public static void Configure(string clinicalUrl, string schedulingUrl)
        {
            _clinicalBaseUrl = clinicalUrl;
            _schedulingBaseUrl = schedulingUrl;
        }

        /// <summary>
        /// Sets the ICD-10 API base URL.
        /// </summary>
        public static void ConfigureIcd10(string icd10Url)
        {
            _icd10BaseUrl = icd10Url;
        }

        /// <summary>
        /// Sets the Gatekeeper API base URL.
        /// </summary>
        public static void ConfigureGatekeeper(string gatekeeperUrl)
        {
            _gatekeeperBaseUrl = gatekeeperUrl;
        }

        // === CLINICAL API ===

        /// <summary>
        /// Fetches all patients from the Clinical API.
        /// </summary>
        public static async Task<Patient[]> GetPatientsAsync()
        {
            var response = await FetchClinicalAsync(_clinicalBaseUrl + "/fhir/Patient");
            return ParseJson<Patient[]>(response);
        }

        /// <summary>
        /// Fetches a patient by ID from the Clinical API.
        /// </summary>
        public static async Task<Patient> GetPatientAsync(string id)
        {
            var response = await FetchClinicalAsync(_clinicalBaseUrl + "/fhir/Patient/" + id);
            return ParseJson<Patient>(response);
        }

        /// <summary>
        /// Searches patients by query string.
        /// </summary>
        public static async Task<Patient[]> SearchPatientsAsync(string query)
        {
            var response = await FetchClinicalAsync(
                _clinicalBaseUrl + "/fhir/Patient/_search?q=" + EncodeUri(query)
            );
            return ParseJson<Patient[]>(response);
        }

        /// <summary>
        /// Fetches encounters for a patient.
        /// </summary>
        public static async Task<Encounter[]> GetEncountersAsync(string patientId)
        {
            var response = await FetchClinicalAsync(
                _clinicalBaseUrl + "/fhir/Patient/" + patientId + "/Encounter"
            );
            return ParseJson<Encounter[]>(response);
        }

        /// <summary>
        /// Fetches conditions for a patient.
        /// </summary>
        public static async Task<Condition[]> GetConditionsAsync(string patientId)
        {
            var response = await FetchClinicalAsync(
                _clinicalBaseUrl + "/fhir/Patient/" + patientId + "/Condition"
            );
            return ParseJson<Condition[]>(response);
        }

        /// <summary>
        /// Fetches medications for a patient.
        /// </summary>
        public static async Task<MedicationRequest[]> GetMedicationsAsync(string patientId)
        {
            var response = await FetchClinicalAsync(
                _clinicalBaseUrl + "/fhir/Patient/" + patientId + "/MedicationRequest"
            );
            return ParseJson<MedicationRequest[]>(response);
        }

        /// <summary>
        /// Creates a new patient.
        /// </summary>
        public static async Task<Patient> CreatePatientAsync(Patient patient)
        {
            var response = await PostClinicalAsync(_clinicalBaseUrl + "/fhir/Patient/", patient);
            return ParseJson<Patient>(response);
        }

        /// <summary>
        /// Updates an existing patient.
        /// </summary>
        public static async Task<Patient> UpdatePatientAsync(string id, Patient patient)
        {
            var response = await PutClinicalAsync(
                _clinicalBaseUrl + "/fhir/Patient/" + id,
                patient
            );
            return ParseJson<Patient>(response);
        }

        // === SCHEDULING API ===

        /// <summary>
        /// Fetches all practitioners from the Scheduling API.
        /// </summary>
        public static async Task<Practitioner[]> GetPractitionersAsync()
        {
            var response = await FetchSchedulingAsync(_schedulingBaseUrl + "/Practitioner");
            return ParseJson<Practitioner[]>(response);
        }

        /// <summary>
        /// Fetches a practitioner by ID from the Scheduling API.
        /// </summary>
        public static async Task<Practitioner> GetPractitionerAsync(string id)
        {
            var response = await FetchSchedulingAsync(_schedulingBaseUrl + "/Practitioner/" + id);
            return ParseJson<Practitioner>(response);
        }

        /// <summary>
        /// Searches practitioners by specialty.
        /// </summary>
        public static async Task<Practitioner[]> SearchPractitionersAsync(string specialty)
        {
            var response = await FetchSchedulingAsync(
                _schedulingBaseUrl + "/Practitioner/_search?specialty=" + EncodeUri(specialty)
            );
            return ParseJson<Practitioner[]>(response);
        }

        /// <summary>
        /// Fetches all appointments from the Scheduling API.
        /// </summary>
        public static async Task<Appointment[]> GetAppointmentsAsync()
        {
            var response = await FetchSchedulingAsync(_schedulingBaseUrl + "/Appointment");
            return ParseJson<Appointment[]>(response);
        }

        /// <summary>
        /// Fetches an appointment by ID from the Scheduling API.
        /// </summary>
        public static async Task<Appointment> GetAppointmentAsync(string id)
        {
            var response = await FetchSchedulingAsync(_schedulingBaseUrl + "/Appointment/" + id);
            return ParseJson<Appointment>(response);
        }

        /// <summary>
        /// Updates an existing appointment.
        /// </summary>
        public static async Task<Appointment> UpdateAppointmentAsync(string id, object appointment)
        {
            var response = await PutSchedulingAsync(
                _schedulingBaseUrl + "/Appointment/" + id,
                appointment
            );
            return ParseJson<Appointment>(response);
        }

        /// <summary>
        /// Fetches appointments for a patient.
        /// </summary>
        public static async Task<Appointment[]> GetPatientAppointmentsAsync(string patientId)
        {
            var response = await FetchSchedulingAsync(
                _schedulingBaseUrl + "/Patient/" + patientId + "/Appointment"
            );
            return ParseJson<Appointment[]>(response);
        }

        /// <summary>
        /// Fetches appointments for a practitioner.
        /// </summary>
        public static async Task<Appointment[]> GetPractitionerAppointmentsAsync(
            string practitionerId
        )
        {
            var response = await FetchSchedulingAsync(
                _schedulingBaseUrl + "/Practitioner/" + practitionerId + "/Appointment"
            );
            return ParseJson<Appointment[]>(response);
        }

        // === ICD-10 API ===

        /// <summary>
        /// Fetches all ICD-10 chapters.
        /// </summary>
        public static async Task<Icd10Chapter[]> GetIcd10ChaptersAsync()
        {
            var response = await FetchIcd10Async(_icd10BaseUrl + "/api/icd10/chapters");
            return ParseJson<Icd10Chapter[]>(response);
        }

        /// <summary>
        /// Fetches blocks for a chapter.
        /// </summary>
        public static async Task<Icd10Block[]> GetIcd10BlocksAsync(string chapterId)
        {
            var response = await FetchIcd10Async(
                _icd10BaseUrl + "/api/icd10/chapters/" + chapterId + "/blocks"
            );
            return ParseJson<Icd10Block[]>(response);
        }

        /// <summary>
        /// Fetches categories for a block.
        /// </summary>
        public static async Task<Icd10Category[]> GetIcd10CategoriesAsync(string blockId)
        {
            var response = await FetchIcd10Async(
                _icd10BaseUrl + "/api/icd10/blocks/" + blockId + "/categories"
            );
            return ParseJson<Icd10Category[]>(response);
        }

        /// <summary>
        /// Fetches codes for a category.
        /// </summary>
        public static async Task<Icd10Code[]> GetIcd10CodesAsync(string categoryId)
        {
            var response = await FetchIcd10Async(
                _icd10BaseUrl + "/api/icd10/categories/" + categoryId + "/codes"
            );
            return ParseJson<Icd10Code[]>(response);
        }

        /// <summary>
        /// Looks up a specific ICD-10 code.
        /// </summary>
        public static async Task<Icd10Code> GetIcd10CodeAsync(string code)
        {
            var response = await FetchIcd10Async(
                _icd10BaseUrl + "/api/icd10/codes/" + EncodeUri(code)
            );
            return ParseJson<Icd10Code>(response);
        }

        /// <summary>
        /// Searches ICD-10 codes by keyword.
        /// </summary>
        public static async Task<Icd10Code[]> SearchIcd10CodesAsync(string query, int limit = 20)
        {
            var response = await FetchIcd10Async(
                _icd10BaseUrl + "/api/icd10/codes?q=" + EncodeUri(query) + "&limit=" + limit
            );
            return ParseJson<Icd10Code[]>(response);
        }

        /// <summary>
        /// Searches ACHI procedure codes by keyword.
        /// </summary>
        public static async Task<AchiCode[]> SearchAchiCodesAsync(string query, int limit = 20)
        {
            var response = await FetchIcd10Async(
                _icd10BaseUrl + "/api/achi/codes?q=" + EncodeUri(query) + "&limit=" + limit
            );
            return ParseJson<AchiCode[]>(response);
        }

        /// <summary>
        /// Performs semantic search using AI embeddings.
        /// </summary>
        public static async Task<SemanticSearchResult[]> SemanticSearchAsync(
            string query,
            int limit = 10,
            bool includeAchi = false
        )
        {
            var request = new SemanticSearchRequest
            {
                Query = query,
                Limit = limit,
                IncludeAchi = includeAchi,
            };
            var response = await PostIcd10Async(_icd10BaseUrl + "/api/search", request);
            var parsed = ParseJson<SemanticSearchResponse>(response);
            return parsed.Results ?? new SemanticSearchResult[0];
        }

        // === HELPER METHODS ===
        // Headers must be built via Script.Write so the literal 'Content-Type'
        // key (with hyphen) survives — C# anonymous-property names like
        // ContentType get emitted as ContentType in JS, which the server then
        // ignores, defaulting the request to text/plain and returning 415.

        private static async Task<string> GetAsync(string url)
        {
            var token = Token;
            var response = await Script.Write<Task<Response>>(@"
                fetch(url, {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Authorization': 'Bearer ' + token
                    }
                })
            ");
            if (!response.Ok)
            {
                throw new Exception("HTTP " + response.Status);
            }
            return await response.Text();
        }

        private static async Task<string> SendJsonAsync(string url, string method, object data)
        {
            var token = Token;
            var body = Script.Call<string>("JSON.stringify", data);
            var response = await Script.Write<Task<Response>>(@"
                fetch(url, {
                    method: method,
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + token
                    },
                    body: body
                })
            ");
            if (!response.Ok)
            {
                throw new Exception("HTTP " + response.Status);
            }
            return await response.Text();
        }

        // Backwards-compatible thin shims so the existing call sites stay readable.
        private static Task<string> FetchIcd10Async(string url) => GetAsync(url);
        private static Task<string> FetchClinicalAsync(string url) => GetAsync(url);
        private static Task<string> FetchSchedulingAsync(string url) => GetAsync(url);
        private static Task<string> PostIcd10Async(string url, object data) => SendJsonAsync(url, "POST", data);
        private static Task<string> PostClinicalAsync(string url, object data) => SendJsonAsync(url, "POST", data);
        private static Task<string> PutClinicalAsync(string url, object data) => SendJsonAsync(url, "PUT", data);
        private static Task<string> PutSchedulingAsync(string url, object data) => SendJsonAsync(url, "PUT", data);

        private static T ParseJson<T>(string json) => Script.Call<T>("JSON.parse", json);

        private static string EncodeUri(string value) =>
            Script.Call<string>("encodeURIComponent", value);
    }

    /// <summary>
    /// Fetch API Response type.
    /// </summary>
    [External]
    [Name("Response")]
    public class Response
    {
        /// <summary>Whether the response was successful.</summary>
        public extern bool Ok { get; }

        /// <summary>HTTP status code.</summary>
        public extern int Status { get; }

        /// <summary>HTTP status text.</summary>
        public extern string StatusText { get; }

        /// <summary>Gets the response body as text.</summary>
        public extern Task<string> Text();

        /// <summary>Gets the response body as JSON.</summary>
        public extern Task<object> Json();
    }
}
