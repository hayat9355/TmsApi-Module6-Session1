using TmsApi.Services;
using TmsApi.Services.Interfaces;
// ↑ This is the only change needed
// Tells the compiler: "IEnrollmentService lives in TmsApi.Services namespace"

public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public void ProcessBatch()
    {
        using var scope = scopeFactory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        // Now it finds Services/IEnrollmentService.cs correctly
    }
}