using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Runs compensation sync: syncs compensation's records between the services that hold them.</summary>
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
            await HandOffNotificationsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off notifications status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayAudiencesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay audiences id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryDocumentFolderStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry document folder status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffGetPayRunsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off get pay runs token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyFeatureFlagsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy feature flags token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayLabelingTaskSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay labeling task subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffBuildingStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off building status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleFeedbacksId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle feedbacks id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayEntitlementMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay entitlement message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncOvertimeMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync overtime message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarrySoftwareLicensesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry software licenses id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardEntitlementDescription.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward entitlement description failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorPrescriptionId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror prescription id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardProblemMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward problem message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarrySelfServiceRequestStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry self service request status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleOnboardingTaskCalculationId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle onboarding task calculation id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffDocumentTemplatesCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off document templates code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorDeliveriesToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror deliveries token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffRostersId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off rosters id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayVisitorPassEligibilityNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay visitor pass eligibility number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayCarePlansStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay care plans status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteChangeRequestRulesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route change request rules id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleDeliveriesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle deliveries message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncRouteOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync route options id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillRouteStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill route status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardShipmentId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward shipment id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
