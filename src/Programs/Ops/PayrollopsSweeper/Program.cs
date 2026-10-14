using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Runs payrollops sweeper: sweepers payrollops's records between the services that hold them.</summary>
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
            await ForwardPendingOvertimesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward pending overtimes status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardArticlesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward articles status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportCertificationsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export certifications id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportLicenseAssignmentWorkflowMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export license assignment workflow message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardVisitorPassNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward visitor pass number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorDevicePolicyLifecycleId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror device policy lifecycle id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleUserStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle user status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardOnboardingPlanNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward onboarding plan notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffConfigSetsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off config sets id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffBudgetsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off budgets message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyItineraryCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy itinerary code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushTicketId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push ticket id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteOfferLifecycleMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route offer lifecycle message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffChangeRequestWorkflowsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off change request workflows status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarrySeatMapsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry seat maps id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayOnboardingTaskSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay onboarding task subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardUserRulesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward user rules id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateRosterRatesNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate roster rates notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillKnowledgeArticleWorkflowMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill knowledge article workflow message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardBenefitPlanId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward benefit plan id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncComplianceScanDecisionStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync compliance scan decision status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryIncidentOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry incident options id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorEscalationCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror escalation code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayOvertimeLifecycleStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay overtime lifecycle status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardCorporateEventReportId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward corporate event report id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportTimesheetsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export timesheets status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportRegistrationReportTimezone.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export registration report timezone failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDocumentFolderVersionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill document folder versions id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
