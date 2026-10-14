using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Runs contractors auditor: auditors contractors's records between the services that hold them.</summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var mesh = Environment.GetEnvironmentVariable("ACME_MESH") ?? "http://localhost:15001";
        using var channel = GrpcChannel.ForAddress(mesh);
        using var deadline = new CancellationTokenSource(TimeSpan.FromHours(2));
        var token = deadline.Token;
        var failed = 0;
        try
        {
            await ForwardEscalationStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward escalation status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayOnboardingPlanMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay onboarding plan message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushTravelPolicyStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push travel policy status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryLearningPathEligibilityMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry learning path eligibility message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardOnboardingTaskLifecycleId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward onboarding task lifecycle id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorCorporateEventApprovalId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror corporate event approval id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillJobOpeningMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill job opening message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillTimesheetWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill timesheet workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorSlaPolicyWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror sla policy workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayGoalWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay goal workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateCorporateEventReportId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate corporate event report id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateBuildingName.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate building name failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffSoftwareLicenseMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off software license message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDocumentFoldersId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill document folders id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayTravelPolicyOptionsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay travel policy options message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDocumentTemplateWorkflowsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill document template workflows token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateOrgUnitId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate org unit id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportMarketingCampaignStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export marketing campaign status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleProblemCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle problem code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffSecretCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off secret code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayVenueCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay venue code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffMediaAssetsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off media assets message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleLicenseAssignmentId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle license assignment id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffTravelPolicyStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off travel policy status failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
