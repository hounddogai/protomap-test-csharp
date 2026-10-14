using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.WorkplaceBackfill;

/// <summary>Runs workplace backfill: backfills workplace's records between the services that hold them.</summary>
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
            await SettleLearningPathRatesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle learning path rates status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardMediaAssetId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward media asset id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardChangeRequestSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward change request subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDatasetsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill datasets message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayLifeEventApprovalStatusId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay life event approval status id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushGetDocumentsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push get documents id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushDemandForecastMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push demand forecast message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushShowsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push shows token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushFeatureViewId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push feature view id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleEscalationExpiryMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle escalation expiry message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayReviewWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay review workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushCustomersStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push customers status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyCourseId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy course id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorCalibrationId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror calibration id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncLoyaltyMemberCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync loyalty member code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarrySignatureVersionId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry signature version id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillChatRoomsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill chat rooms id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushShiftMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push shift message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteOnboardingTaskMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route onboarding task message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushChangeRequestMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push change request message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleJobOpeningEligibilityId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle job opening eligibility id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
