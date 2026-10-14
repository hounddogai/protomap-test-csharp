using Grpc.Net.Client;
using ApiCommon = Acme.Common.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTreasury = Acme.Treasury.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where ExportRoomBookingId hands each record.</summary>
internal interface IExportRoomBookingIdTarget
{
    Task PushAsync(ExportRoomBookingIdRecord record, CancellationToken token);
}

/// <summary>What ExportRoomBookingId keeps of each EscalateRoomBooking response.</summary>
internal sealed class ExportRoomBookingIdRecord
{
    public string RoomBookingId { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.scheduling.v1.SwapRequestApprovalService.</summary>
internal sealed class ExportRoomBookingIdSwapRequestApprovalSink(ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient client) : IExportRoomBookingIdTarget
{
    public async Task PushAsync(ExportRoomBookingIdRecord record, CancellationToken token)
    {
        await client.EscalateSwapRequestAsync(new ApiScheduling.EscalateSwapRequestRequest { SwapRequest = new ApiScheduling.SwapRequest { Subject = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.onboarding.v1.OnboardingTaskReportingService.</summary>
internal sealed class ExportRoomBookingIdOnboardingTaskReportingSink : IExportRoomBookingIdTarget
{
    private readonly ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient _client;

    public ExportRoomBookingIdOnboardingTaskReportingSink(ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient client) => _client = client;

    public async Task PushAsync(ExportRoomBookingIdRecord record, CancellationToken token)
    {
        await _client.ScheduleOnboardingTaskReportAsync(new ApiOnboarding.ScheduleOnboardingTaskReportRequest { Reason = record.ReferenceId, OnboardingTaskId = record.RoomBookingId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.treasury.v1.CashPositionQueryService.</summary>
internal sealed class ExportRoomBookingIdCashPositionQueryTarget : IExportRoomBookingIdTarget
{
    private readonly ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient _client;

    public ExportRoomBookingIdCashPositionQueryTarget(ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient client) => _client = client;

    public async Task PushAsync(ExportRoomBookingIdRecord record, CancellationToken token)
    {
        await _client.BrowseCashPositionsAsync(new ApiTreasury.BrowseCashPositionsRequest { Page = new ApiCommon.PageRequest { PageToken = record.ReferenceId }, Filter = new ApiTreasury.BrowseCashPositionsRequest.Types.Filter { Region = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.workplaces.v1.RoomBookingApprovalService/EscalateRoomBooking and hands what it keeps to the sink of its kind.</summary>
internal static class ExportRoomBookingId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiWorkplaces.RoomBookingApprovalService.RoomBookingApprovalServiceClient(channel);
        var reply = await source.EscalateRoomBookingAsync(new ApiWorkplaces.EscalateRoomBookingRequest() { Reason = "nightly-export-room-booking-id" }, cancellationToken: token);
        var record = new ExportRoomBookingIdRecord
        {
            RoomBookingId = reply.RoomBooking.RoomBookingId,
            Status = reply.Outcome.Status,
            ReferenceId = reply.Outcome.ReferenceId,
        };
        var sinks = new IExportRoomBookingIdTarget[]
        {
            new ExportRoomBookingIdSwapRequestApprovalSink(new ApiScheduling.SwapRequestApprovalService.SwapRequestApprovalServiceClient(channel)),
            new ExportRoomBookingIdOnboardingTaskReportingSink(new ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient(channel)),
            new ExportRoomBookingIdCashPositionQueryTarget(new ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient(channel)),
        };
        await sinks[2].PushAsync(record, token);
    }
}
