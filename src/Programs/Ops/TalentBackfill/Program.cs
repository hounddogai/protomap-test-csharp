using Grpc.Core;
using Grpc.Net.Client;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Runs talent backfill: backfills talent's records between the services that hold them.</summary>
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
            await ExportOnboardingTaskSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export onboarding task subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffAppointmentStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off appointment status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RelayLifeEventOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"relay life event options id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteMediaAssetMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route media asset message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await RouteRiskRegistersId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"route risk registers id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffProfilesToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off profiles token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillRiskRegistersStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill risk registers status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ForwardShipmentsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"forward shipments status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushChatRoomId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push chat room id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportWorkOrderSubject.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export work order subject failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncSearchIndexesStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync search indexes status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayMetricSeriesMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay metric series message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateCustomerId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate customer id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await SyncPaymentMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync payment message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyLabelingTaskOptionsId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy labeling task options id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushEmploymentContractStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push employment contract status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await MirrorLabelingTasksToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"mirror labeling tasks token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateCarePlanId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate care plan id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillAuditEventsStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill audit events status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await CopyDevicePolicyExpiryId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"copy device policy expiry id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await BackfillChatRoomMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"backfill chat room message failed: {error.Status.Detail}");
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
            await SyncAudiencesId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"sync audiences id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateDriverStatus.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate driver status failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PushFuelCardMessage.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"push fuel card message failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ExportItineraryApprovalNotes.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"export itinerary approval notes failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await PropagateEmployeeId.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"propagate employee id failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await ReplayYardsToken.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"replay yards token failed: {error.Status.Detail}");
            failed++;
        }
        try
        {
            await HandOffChangeRequestsUri.RunAsync(channel, token);
        }
        catch (RpcException error)
        {
            Console.Error.WriteLine($"hand off change requests uri failed: {error.Status.Detail}");
            failed++;
        }
        return failed == 0 ? 0 : 1;
    }
}
