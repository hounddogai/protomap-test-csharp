using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Runs assets replicator: replicators assets's records between the services that hold them.</summary>
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
            await CopyCorporateEventRatesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy corporate event rates id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncTravelPolicyWorkflowsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync travel policy workflows status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardCryptoKeyMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward crypto key message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncInterviewWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync interview workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayFeedbackWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay feedback workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportBoxOfficeSaleNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export box office sale notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorCorporateEventWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror corporate event workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryIncidentsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry incidents token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportOnboardingTaskWorkflowSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export onboarding task workflow subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryOvertimeMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry overtime message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportCorporateEventMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export corporate event message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportEscalationStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export escalation status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncPrescriptionMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync prescription message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncJobOpeningWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync job opening workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleCandidateStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle candidate status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncServiceAssetOptionsCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync service asset options code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorAudiencesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror audiences id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayOrgUnitWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay org unit workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateFeatureFlagsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate feature flags message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillManagedDeviceMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill managed device message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyTripEligibilityId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy trip eligibility id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorSelfServiceRequestsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror self service requests status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportRoomBookingId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export room booking id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushOnboardingPlanApprovalId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push onboarding plan approval id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteAudienceName.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route audience name failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayEntitlementWorkflowMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay entitlement workflow message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorFeatureViewStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror feature view status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayMetricSeriesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay metric series id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorChangeRequestRatesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror change request rates message failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
