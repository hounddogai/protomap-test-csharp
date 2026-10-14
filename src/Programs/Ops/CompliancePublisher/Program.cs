using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Runs compliance publisher: publishers compliance's records between the services that hold them.</summary>
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
            await PushTravelBookingCalculationStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push travel booking calculation status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorCertificateId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror certificate id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillClockEventId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill clock event id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteRegistrationWorkflowStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route registration workflow status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateRoomBookingWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate room booking workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillAuditEventMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill audit event message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryCoursesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry courses message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayCalibrationStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay calibration status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushPayRunNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push pay run notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncLicenseAssignmentWorkflowId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync license assignment workflow id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorTripNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror trip notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SettleEntitlementStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"settle entitlement status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyAccessPoliciesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy access policies status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CarryNotificationSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"carry notification subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorConfigSetId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror config set id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushFeatureViewOptionsCode.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push feature view options code failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayCandidateEligibilityId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay candidate eligibility id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillCertificationMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill certification message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffTravelBookingsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off travel bookings message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateGoalsMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate goals message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardWorkOrderCalculationId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward work order calculation id failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
