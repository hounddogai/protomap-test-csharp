using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Runs compensation importer: importers compensation's records between the services that hold them.</summary>
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
            await BackfillIncidentsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill incidents id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDispatchOrderStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill dispatch order status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyOrderCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy order code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportTripRatesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export trip rates message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleCheckoutSessionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle checkout sessions id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleEscalationReportId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle escalation report id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyPatientsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy patients message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorDocumentsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror documents status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncProfilesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync profiles message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayServiceAssetCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay service asset code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushEquipmentWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push equipment workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayEnrollmentApprovalStatusStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay enrollment approval status status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffProfileMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off profile message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardDemandForecastId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward demand forecast id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateSecretId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate secret id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleCourseApprovalStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle course approval status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyBankAccountsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy bank accounts id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteTravelBookingNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route travel booking notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryOrgUnitWorkflowMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry org unit workflow message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateKnowledgeArticleCalculationNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate knowledge article calculation number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportGetAppointmentsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export get appointments status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncLabelingTasksMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync labeling tasks message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorSignatureExportMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror signature export message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillOrgUnitName.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill org unit name failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
