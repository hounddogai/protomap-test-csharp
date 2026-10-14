using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Runs absence reconciler: reconcilers absence's records between the services that hold them.</summary>
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
            await BackfillRegistrationMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill registration message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushRoomBookingLifecycleStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push room booking lifecycle status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportProfilesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export profiles id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillSwapRequestOptionsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill swap request options status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardPendingDependentsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward pending dependents message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncInterviewWorkflowsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync interview workflows message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteBenefitPlanId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route benefit plan id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorOrgUnitWorkflowMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror org unit workflow message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushVenueMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push venue message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyFuelCardsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy fuel cards status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardPromotionDescription.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward promotion description failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillDatasetId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill dataset id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteAdmissionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route admissions id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportTelemetryReadingsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export telemetry readings message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyRouteBindingsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy route bindings id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffTicketSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off ticket subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateAccessPolicyRulesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate access policy rules id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorClockEventMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror clock event message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorExperimentsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror experiments status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyCartId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy cart id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayTravelPolicyConflictStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay travel policy conflict status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushNotificationsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push notifications status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagatePrescriptionRatesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate prescription rates status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleSecretsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle secrets message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateServiceAssetReportId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate service asset report id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorDeskWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror desk workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateOnboardingTasksCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate onboarding tasks code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncDocumentVersionsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync document versions status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncTelemetryReadingsKind.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync telemetry readings kind failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
