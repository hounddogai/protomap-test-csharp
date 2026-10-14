using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiFeaturestore = Acme.Featurestore.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where PushFeatureViewOptionsCode hands each record.</summary>
internal interface IPushFeatureViewOptionsCodeDestination
{
    Task ApplyAsync(PushFeatureViewOptionsCodeRecord record, CancellationToken token);
}

/// <summary>What PushFeatureViewOptionsCode keeps of each CompareFeatureViewOptions response.</summary>
internal sealed class PushFeatureViewOptionsCodeRecord
{
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.learning.v1.CourseApprovalService.</summary>
internal sealed class PushFeatureViewOptionsCodeCourseApprovalTarget : IPushFeatureViewOptionsCodeDestination
{
    private readonly ApiLearning.CourseApprovalService.CourseApprovalServiceClient _client;

    public PushFeatureViewOptionsCodeCourseApprovalTarget(ApiLearning.CourseApprovalService.CourseApprovalServiceClient client) => _client = client;

    public async Task ApplyAsync(PushFeatureViewOptionsCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.ApproveCourseRequest { Course = new ApiLearning.Course { Name = record.Code }, Reason = record.Message }, token, _client.ApproveCourseAsync);
    }
}

/// <summary>Passes records to acme.invoicing.v1.InvoiceBatchCalculationService.</summary>
internal sealed class PushFeatureViewOptionsCodeInvoiceBatchCalculationWriter : IPushFeatureViewOptionsCodeDestination
{
    private readonly ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient _client;

    public PushFeatureViewOptionsCodeInvoiceBatchCalculationWriter(ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(PushFeatureViewOptionsCodeRecord record, CancellationToken token)
    {
        await _client.EstimateInvoiceBatchAsync(new ApiInvoicing.EstimateInvoiceBatchRequest { InvoiceBatchId = record.Code }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employeeportal.v1.PortalHomeIntegrationService.</summary>
internal sealed class PushFeatureViewOptionsCodePortalHomeIntegrationRelay : IPushFeatureViewOptionsCodeDestination
{
    private readonly ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient _client;

    public PushFeatureViewOptionsCodePortalHomeIntegrationRelay(ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient client) => _client = client;

    public async Task ApplyAsync(PushFeatureViewOptionsCodeRecord record, CancellationToken token)
    {
        await _client.ConnectPortalHomeSourceAsync(new ApiEmployeeportal.ConnectPortalHomeSourceRequest { Reason = record.Code, Details = new ApiEmployeeportal.ConnectPortalHomeSourceRequest.Types.Details { SourceSystem = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.timekeeping.v1.ClockEventValidationService.</summary>
internal sealed class PushFeatureViewOptionsCodeClockEventValidationTarget(ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient client) : IPushFeatureViewOptionsCodeDestination
{
    public async Task ApplyAsync(PushFeatureViewOptionsCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.ValidateClockEventRequest { ClockEventId = record.Code }, token, client.ValidateClockEventAsync);
    }
}

/// <summary>Reads acme.featurestore.v1.FeatureViewCalculationService/CompareFeatureViewOptions and hands what it keeps to the sink of its kind.</summary>
internal static class PushFeatureViewOptionsCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiFeaturestore.FeatureViewCalculationService.FeatureViewCalculationServiceClient(channel);
        var got = await source.CompareFeatureViewOptionsAsync(new ApiFeaturestore.CompareFeatureViewOptionsRequest() { FeatureViewId = "current-push-feature-view-options-code" }, cancellationToken: token);
        var record = new PushFeatureViewOptionsCodeRecord
        {
            Code = Text.Normalize(got.FeatureView.Code),
            Message = got.Result.Message,
        };
        var sinks = new Dictionary<string, IPushFeatureViewOptionsCodeDestination>
        {
            ["course-approval"] = new PushFeatureViewOptionsCodeCourseApprovalTarget(new ApiLearning.CourseApprovalService.CourseApprovalServiceClient(channel)),
            ["invoice-batch-calculation"] = new PushFeatureViewOptionsCodeInvoiceBatchCalculationWriter(new ApiInvoicing.InvoiceBatchCalculationService.InvoiceBatchCalculationServiceClient(channel)),
            ["portal-home-integration"] = new PushFeatureViewOptionsCodePortalHomeIntegrationRelay(new ApiEmployeeportal.PortalHomeIntegrationService.PortalHomeIntegrationServiceClient(channel)),
            ["clock-event-validation"] = new PushFeatureViewOptionsCodeClockEventValidationTarget(new ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "course-approval")
                continue;
            await sink.ApplyAsync(record, token);
        }
    }
}
