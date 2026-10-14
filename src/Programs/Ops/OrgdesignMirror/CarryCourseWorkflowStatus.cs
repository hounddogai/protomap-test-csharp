using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTicketing = Acme.Ticketing.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where CarryCourseWorkflowStatus hands each record.</summary>
internal interface ICarryCourseWorkflowStatusDestination
{
    Task ForwardAsync(CarryCourseWorkflowStatusRecord record, CancellationToken token);
}

/// <summary>What CarryCourseWorkflowStatus keeps of each PauseCourseWorkflow response.</summary>
internal sealed class CarryCourseWorkflowStatusRecord
{
    public string Status { get; init; } = "";
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestValidationService.</summary>
internal sealed class CarryCourseWorkflowStatusChangeRequestValidationWriter : ICarryCourseWorkflowStatusDestination
{
    private readonly ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient _client;

    public CarryCourseWorkflowStatusChangeRequestValidationWriter(ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryCourseWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiItsm.CheckChangeRequestEligibilityRequest { Details = new ApiItsm.CheckChangeRequestEligibilityRequest.Types.Details { Locale = record.Code }, ChangeRequest = new ApiItsm.ChangeRequest { Subject = record.Status }, ChangeRequestId = record.Message }, token, _client.CheckChangeRequestEligibilityAsync);
    }
}

/// <summary>Sends records to acme.workplaces.v1.VisitorPassLifecycleService.</summary>
internal sealed class CarryCourseWorkflowStatusVisitorPassLifecycleSink(ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient client) : ICarryCourseWorkflowStatusDestination
{
    public async Task ForwardAsync(CarryCourseWorkflowStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiWorkplaces.GetVisitorPassLifecycleRequest { VisitorPassId = record.Status }, token, client.GetVisitorPassLifecycleAsync);
    }
}

/// <summary>Hands records to acme.ticketing.v1.TicketService.</summary>
internal sealed class CarryCourseWorkflowStatusTicketSink : ICarryCourseWorkflowStatusDestination
{
    private readonly ApiTicketing.TicketService.TicketServiceClient _client;

    public CarryCourseWorkflowStatusTicketSink(ApiTicketing.TicketService.TicketServiceClient client) => _client = client;

    public async Task ForwardAsync(CarryCourseWorkflowStatusRecord record, CancellationToken token)
    {
        await _client.CreateTicketAsync(new ApiTicketing.CreateTicketRequest { Ticket = new ApiTicketing.Ticket { Number = record.Message, TicketId = record.Status, Notes = record.Code } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.learning.v1.CourseWorkflowService/PauseCourseWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class CarryCourseWorkflowStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLearning.CourseWorkflowService.CourseWorkflowServiceClient(channel);
        var response = await source.PauseCourseWorkflowAsync(new ApiLearning.PauseCourseWorkflowRequest() { Reason = "default-carry-course-workflow-status" }, cancellationToken: token);
        var record = new CarryCourseWorkflowStatusRecord
        {
            Status = Text.Normalize(response.Outcome.Status),
            Code = response.Course.Code,
            Message = Text.Normalize(response.Outcome.Message),
        };
        var sinks = new Dictionary<string, ICarryCourseWorkflowStatusDestination>
        {
            ["change-request-validation"] = new CarryCourseWorkflowStatusChangeRequestValidationWriter(new ApiItsm.ChangeRequestValidationService.ChangeRequestValidationServiceClient(channel)),
            ["visitor-pass-lifecycle"] = new CarryCourseWorkflowStatusVisitorPassLifecycleSink(new ApiWorkplaces.VisitorPassLifecycleService.VisitorPassLifecycleServiceClient(channel)),
            ["ticket"] = new CarryCourseWorkflowStatusTicketSink(new ApiTicketing.TicketService.TicketServiceClient(channel)),
        };
        await RouteCarryCourseWorkflowStatusAsync("ticket", record, sinks, token);
    }

    private static async Task RouteCarryCourseWorkflowStatusAsync(string kind, CarryCourseWorkflowStatusRecord record, IReadOnlyDictionary<string, ICarryCourseWorkflowStatusDestination> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ForwardAsync(record, token);
    }
}
