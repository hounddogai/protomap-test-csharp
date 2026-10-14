using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Runs absence relay: relays absence's records between the services that hold them.</summary>
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
            await MirrorOfferStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror offer status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDesksStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill desks status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushLoyaltyMembersId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push loyalty members id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryCorporateEventReportsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry corporate event reports status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncRoomBookingOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync room booking options id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateVenueCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate venue code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncOnboardingPlanStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync onboarding plan status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyDocumentTemplateWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy document template workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillCertificatesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill certificates status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffDocumentFolderRulesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off document folder rules id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyDeliveryMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy delivery message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteTravelPolicyApprovalStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route travel policy approval status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncCourseStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync course status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateCorporateEventApprovalStatusTitle.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate corporate event approval status title failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleEquipmentWorkflowsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle equipment workflows id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncKnowledgeArticleApprovalNumber.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync knowledge article approval number failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncDependentStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync dependent status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillTravelBookingLifecycleId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill travel booking lifecycle id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayOnboardingTaskId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay onboarding task id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncCartsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync carts id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
