using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Runs orgdesign mirror: mirrors orgdesign's records between the services that hold them.</summary>
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
            await RelayRegistrationsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay registrations status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettlePrescriptionId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle prescription id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushCertificationId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push certification id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncCorporateEventTotalsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync corporate event totals status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayBudgetId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay budget id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorDeliveryId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror delivery id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportClockEventId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export clock event id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardRosterStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward roster status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleEnrollmentId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle enrollment id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorRegistrationWorkflowMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror registration workflow message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncTicketId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync ticket id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryCourseWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry course workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyTripId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy trip id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorBuildingsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror buildings message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorTripsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror trips status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagatePendingLearningPathsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate pending learning paths token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleManagedDeviceWorkflowCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle managed device workflow code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncSlaPolicyOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync sla policy options id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyTicketCalculationMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy ticket calculation message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardRateLimitOptionsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward rate limit options status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffGetAdmissionsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off get admissions status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteDocumentTestNoticeNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route document test notice notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteCarePlansUri.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route care plans uri failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillComplianceScanId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill compliance scan id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
