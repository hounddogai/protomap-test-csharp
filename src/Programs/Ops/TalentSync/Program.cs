using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Runs talent sync: syncs talent's records between the services that hold them.</summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var mesh = Environment.GetEnvironmentVariable("ACME_MESH") ?? "http://localhost:15001";
        using var channel = GrpcChannel.ForAddress(mesh);
        using var deadline = new CancellationTokenSource(TimeSpan.FromHours(4));
        var token = deadline.Token;
        var failed = 0;
        try
        {
            await SettleDeskId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle desk id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardSlaPolicyName.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward sla policy name failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardSponsorId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward sponsor id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteEmployeeStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route employee status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayCandidatesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay candidates message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayRegistrationsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay registrations id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffOvertimeOptionsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off overtime options message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayFeatureFlagStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay feature flag status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateTimesheetNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate timesheet number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteVisitorPassReportsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route visitor pass reports status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyLifeEventStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy life event status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayLicenseAssignmentReportTimezone.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay license assignment report timezone failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncTravelPolicyId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync travel policy id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncAccessPoliciesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync access policies message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardSubscriptionsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward subscriptions status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorVideoVisitsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror video visits id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteOnboardingTasksStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route onboarding tasks status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillInvoiceBatchCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill invoice batch code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplaySignaturesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay signatures status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryDeskId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry desk id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyDocumentRulesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy document rules id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportPendingTripsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export pending trips id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillCarePlanId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill care plan id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushDeskOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push desk options id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
