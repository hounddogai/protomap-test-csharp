using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTours = Acme.Tours.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where ExportLicenseAssignmentWorkflowMessage hands each record.</summary>
internal interface IExportLicenseAssignmentWorkflowMessageTarget
{
    Task SendAsync(ExportLicenseAssignmentWorkflowMessageRecord record, CancellationToken token);
}

/// <summary>What ExportLicenseAssignmentWorkflowMessage keeps of each RetryLicenseAssignmentWorkflow response.</summary>
internal sealed class ExportLicenseAssignmentWorkflowMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string LicenseAssignmentId { get; init; } = "";
}

/// <summary>Hands records to acme.cart.v1.CartService.</summary>
internal sealed class ExportLicenseAssignmentWorkflowMessageCartRelay(ApiCart.CartService.CartServiceClient client) : IExportLicenseAssignmentWorkflowMessageTarget
{
    public async Task SendAsync(ExportLicenseAssignmentWorkflowMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCart.ListCartsRequest { Filter = new ApiCart.ListCartsRequest.Types.Filter { Region = record.LicenseAssignmentId }, Page = new ApiCommon.PageRequest { PageToken = record.Status } }, token, client.ListCartsAsync);
    }
}

/// <summary>Writes records to acme.tours.v1.TourQueryService.</summary>
internal sealed class ExportLicenseAssignmentWorkflowMessageTourQueryTarget : IExportLicenseAssignmentWorkflowMessageTarget
{
    private readonly ApiTours.TourQueryService.TourQueryServiceClient _client;

    public ExportLicenseAssignmentWorkflowMessageTourQueryTarget(ApiTours.TourQueryService.TourQueryServiceClient client) => _client = client;

    public async Task SendAsync(ExportLicenseAssignmentWorkflowMessageRecord record, CancellationToken token)
    {
        await _client.LookupTourAsync(new ApiTours.LookupTourRequest { TourId = record.LicenseAssignmentId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ExportLicenseAssignmentWorkflowMessageLogOnly : IExportLicenseAssignmentWorkflowMessageTarget
{
    public Task SendAsync(ExportLicenseAssignmentWorkflowMessageRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Message}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.licensing.v1.LicenseAssignmentWorkflowService/RetryLicenseAssignmentWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ExportLicenseAssignmentWorkflowMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLicensing.LicenseAssignmentWorkflowService.LicenseAssignmentWorkflowServiceClient(channel);
        var got = await source.RetryLicenseAssignmentWorkflowAsync(new ApiLicensing.RetryLicenseAssignmentWorkflowRequest() { Reason = "default-export-license-assignment-workflow-message" }, cancellationToken: token);
        var record = new ExportLicenseAssignmentWorkflowMessageRecord
        {
            Message = got.Result.Message,
            Status = got.Outcome.Status,
            LicenseAssignmentId = Text.Normalize(got.LicenseAssignment.LicenseAssignmentId),
        };
        await DeliverExportLicenseAssignmentWorkflowMessageAsync("tour-query", record, channel, token);
    }

    private static async Task DeliverExportLicenseAssignmentWorkflowMessageAsync(string kind, ExportLicenseAssignmentWorkflowMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        IExportLicenseAssignmentWorkflowMessageTarget sink = new ExportLicenseAssignmentWorkflowMessageLogOnly();
        switch (kind)
        {
            case "cart":
                sink = new ExportLicenseAssignmentWorkflowMessageCartRelay(new ApiCart.CartService.CartServiceClient(channel));
                break;
            case "tour-query":
                sink = new ExportLicenseAssignmentWorkflowMessageTourQueryTarget(new ApiTours.TourQueryService.TourQueryServiceClient(channel));
                break;
        }
        await sink.SendAsync(record, token);
    }
}
