using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Runs payrollops relay: relays payrollops's records between the services that hold them.</summary>
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
            await CopyEmployeesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy employees id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportTimesheetNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export timesheet notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffDocumentFolderVersionMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off document folder version message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayBuildingLifecycleId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay building lifecycle id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncTripsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync trips id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardPipelineId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward pipeline id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillOnboardingPlanDecisionStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill onboarding plan decision status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffFuelCardStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off fuel card status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayCandidateId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay candidate id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardDocumentTemplateRatesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward document template rates status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushTripWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push trip workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportPatientMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export patient message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyAnnouncementsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy announcements status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleAudienceId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle audience id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateDevicePoliciesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate device policies message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryWarehouseMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry warehouse message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushOfferReportId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push offer report id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayYardStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay yard status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteChangeRequestsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route change requests message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportCandidatesToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export candidates token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillSlaPolicyWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill sla policy workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleIncidentId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle incident id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushInterviewsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push interviews id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffWebhookEndpointMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off webhook endpoint message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteArticlesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route articles id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyLedgerAccountCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy ledger account code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayTimesheetWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay timesheet workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryVisitorPassId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry visitor pass id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
