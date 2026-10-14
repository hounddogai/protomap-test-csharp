using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.AssetsSweeper;

/// <summary>Where SyncYardName hands each record.</summary>
internal interface ISyncYardNameSink
{
    Task ForwardAsync(SyncYardNameRecord record, CancellationToken token);
}

/// <summary>What SyncYardName keeps of each SimulateYard response.</summary>
internal sealed class SyncYardNameRecord
{
    public string Name { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string YardId { get; init; } = "";
}

/// <summary>Sends records to acme.helpdesk.v1.TicketApprovalService.</summary>
internal sealed class SyncYardNameTicketApprovalTarget : ISyncYardNameSink
{
    private readonly ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient _client;

    public SyncYardNameTicketApprovalTarget(ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncYardNameRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.GetTicketApprovalStatusRequest { TicketId = record.Name }, token, _client.GetTicketApprovalStatusAsync);
    }
}

/// <summary>Sends records to acme.licensing.v1.SoftwareLicenseWorkflowService.</summary>
internal sealed class SyncYardNameSoftwareLicenseWorkflowTarget(ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient client) : ISyncYardNameSink
{
    public async Task ForwardAsync(SyncYardNameRecord record, CancellationToken token)
    {
        await client.SignalSoftwareLicenseWorkflowAsync(new ApiLicensing.SignalSoftwareLicenseWorkflowRequest { SoftwareLicense = new ApiLicensing.SoftwareLicense { SoftwareLicenseId = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.devices.v1.DevicePolicyCalculationService.</summary>
internal sealed class SyncYardNameDevicePolicyCalculationSink : ISyncYardNameSink
{
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _client;

    public SyncYardNameDevicePolicyCalculationSink(ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncYardNameRecord record, CancellationToken token)
    {
        await _client.GetDevicePolicyRatesAsync(new ApiDevices.GetDevicePolicyRatesRequest { DevicePolicyId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.performance.v1.FeedbackApprovalService.</summary>
internal sealed class SyncYardNameFeedbackApprovalSink : ISyncYardNameSink
{
    private readonly ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient _client;

    public SyncYardNameFeedbackApprovalSink(ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(SyncYardNameRecord record, CancellationToken token)
    {
        await _client.ListPendingFeedbacksAsync(new ApiPerformance.ListPendingFeedbacksRequest { OrderBy = record.Name, Filter = new ApiPerformance.ListPendingFeedbacksRequest.Types.Filter { OwnerId = record.Status, Region = record.YardId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.yards.v1.YardCalculationService/SimulateYard and hands what it keeps to the sink of its kind.</summary>
internal static class SyncYardName
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiYards.YardCalculationService.YardCalculationServiceClient(channel);
        var got = await source.SimulateYardAsync(new ApiYards.SimulateYardRequest() { Reason = "nightly-sync-yard-name" }, cancellationToken: token);
        var record = new SyncYardNameRecord
        {
            Name = Text.Normalize(got.Yard.Name),
            Message = got.Result.Message,
            Status = Text.Normalize(got.Result.Status),
            YardId = got.Yard.YardId,
        };
        var sinks = new List<ISyncYardNameSink>
        {
            new SyncYardNameTicketApprovalTarget(new ApiHelpdesk.TicketApprovalService.TicketApprovalServiceClient(channel)),
            new SyncYardNameSoftwareLicenseWorkflowTarget(new ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient(channel)),
            new SyncYardNameDevicePolicyCalculationSink(new ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient(channel)),
            new SyncYardNameFeedbackApprovalSink(new ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.ForwardAsync(record, token);
    }
}
