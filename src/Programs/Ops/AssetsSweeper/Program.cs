using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Runs assets sweeper: sweepers assets's records between the services that hold them.</summary>
public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var mesh = Environment.GetEnvironmentVariable("ACME_MESH") ?? "http://localhost:15001";
        using var channel = GrpcChannel.ForAddress(mesh);
        using var deadline = new CancellationTokenSource(TimeSpan.FromHours(1));
        var token = deadline.Token;
        var failed = 0;
        try
        {
            await RelayOrgUnitsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay org units status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportPoliciesTimezone.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export policies timezone failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayVisitorPassId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay visitor pass id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateDocumentsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate documents id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleGetRegisteredModelsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle get registered models status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyOrgUnitWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy org unit workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteMetricSeriesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route metric series id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryRecommendationSetsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry recommendation sets message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorEnrollmentsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror enrollments id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateSignatureExportId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate signature export id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateSelfServiceRequestEventId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate self service request event id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateRosterMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate roster message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardConversationsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward conversations token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillEntitlementCalculationId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill entitlement calculation id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateCorporateEventReportTimezone.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate corporate event report timezone failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleProblemStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle problem status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyFeedbackDecisionMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy feedback decision message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardTicketDecisionId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward ticket decision id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncSwapRequestMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync swap request message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncYardName.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync yard name failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateCorporateEventStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate corporate event status failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
