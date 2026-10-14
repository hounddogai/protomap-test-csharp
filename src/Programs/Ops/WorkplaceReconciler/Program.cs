using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Runs workplace reconciler: reconcilers workplace's records between the services that hold them.</summary>
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
            await SyncLedgerAccountsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync ledger accounts token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryDocumentSyncStateNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry document sync state number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyGoalWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy goal workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteDocumentFoldersStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route document folders status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayTravelBookingId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay travel booking id failed: {error.Status.Detail}");
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
            await BackfillOvertimeMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill overtime message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayBuildingsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay buildings id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncPaymentId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync payment id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffWorkOrderCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off work order code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillRecommendationSetNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill recommendation set number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryTimesheetId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry timesheet id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushDocumentTemplateWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push document template workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushInvoiceBatchId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push invoice batch id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillKnowledgeArticleStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill knowledge article status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushRegistrationWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push registration workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffLifeEventId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off life event id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncInterviewCalculationMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync interview calculation message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardDependentWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward dependent workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillJobOpeningNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill job opening number failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
