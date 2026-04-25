namespace ClinicalCoding.TestSupport;

public static class HttpResponseAssertions
{
    public static async System.Threading.Tasks.Task ShouldHaveStatusAsync(
        this System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask,
        System.Net.HttpStatusCode expectedStatusCode
    )
    {
        using var response = await responseTask.WithStatusAsync(expectedStatusCode);
    }

    public static async System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> WithStatusAsync(
        this System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage> responseTask,
        System.Net.HttpStatusCode expectedStatusCode
    )
    {
        var response = await responseTask;

        Xunit.Assert.Equal(expectedStatusCode, response.StatusCode);

        return response;
    }
}
