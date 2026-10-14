using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.EngagementExporter;

/// <summary>Runs engagement exporter: exporters engagement's records between the services that hold them.</summary>
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
            await SyncNotificationRulesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync notification rules id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagatePriceListsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate price lists id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayCertificationWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay certification workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayDocumentVersionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay document versions id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorProblemConflictId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror problem conflict id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportOrgUnitApprovalStatusStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export org unit approval status status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleRosterMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle roster message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteCandidateId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route candidate id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDriverId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill driver id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushTicketRulesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push ticket rules message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteIncidentRulesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route incident rules status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayItineraryReportNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay itinerary report notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryAuditEventsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry audit events message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteEquipmentWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route equipment workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateConversationCalculationStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate conversation calculation status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardWorkOrdersStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward work orders status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardTimesheetWorkflowsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward timesheet workflows status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleCorporateEventsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle corporate events message failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
