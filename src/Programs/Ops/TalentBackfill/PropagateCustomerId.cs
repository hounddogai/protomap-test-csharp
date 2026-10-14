using Grpc.Net.Client;
using ApiCustomers = Acme.Customers.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PropagateCustomerId hands each record.</summary>
internal interface IPropagateCustomerIdSink
{
    Task WriteAsync(PropagateCustomerIdRecord record, CancellationToken token);
}

/// <summary>What PropagateCustomerId keeps of each UpdateCustomer response.</summary>
internal sealed class PropagateCustomerIdRecord
{
    public string CustomerId { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentApprovalService.</summary>
internal sealed class PropagateCustomerIdCourseEnrollmentApprovalWriter(ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient client) : IPropagateCustomerIdSink
{
    public async Task WriteAsync(PropagateCustomerIdRecord record, CancellationToken token)
    {
        await client.CommentCourseEnrollmentApprovalAsync(new ApiLearning.CommentCourseEnrollmentApprovalRequest { CourseEnrollment = new ApiLearning.CourseEnrollment { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.workplaces.v1.BuildingLifecycleService.</summary>
internal sealed class PropagateCustomerIdBuildingLifecycleRelay : IPropagateCustomerIdSink
{
    private readonly ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient _client;

    public PropagateCustomerIdBuildingLifecycleRelay(ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateCustomerIdRecord record, CancellationToken token)
    {
        await _client.SuspendBuildingAsync(new ApiWorkplaces.SuspendBuildingRequest { Details = new ApiWorkplaces.SuspendBuildingRequest.Types.Details { ReasonCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.itsm.v1.ChangeRequestReportingService.</summary>
internal sealed class PropagateCustomerIdChangeRequestReportingTarget : IPropagateCustomerIdSink
{
    private readonly ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient _client;

    public PropagateCustomerIdChangeRequestReportingTarget(ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateCustomerIdRecord record, CancellationToken token)
    {
        await _client.GenerateChangeRequestReportAsync(new ApiItsm.GenerateChangeRequestReportRequest { Details = new ApiItsm.GenerateChangeRequestReportRequest.Types.Details { Cron = record.CorrelationId }, ChangeRequestId = record.CustomerId, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.performance.v1.FeedbackWorkflowService.</summary>
internal sealed class PropagateCustomerIdFeedbackWorkflowSink : IPropagateCustomerIdSink
{
    private readonly ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient _client;

    public PropagateCustomerIdFeedbackWorkflowSink(ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient client) => _client = client;

    public async Task WriteAsync(PropagateCustomerIdRecord record, CancellationToken token)
    {
        await _client.InspectFeedbackWorkflowAsync(new ApiPerformance.InspectFeedbackWorkflowRequest { FeedbackId = record.CustomerId }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateCustomerIdLogOnly : IPropagateCustomerIdSink
{
    public Task WriteAsync(PropagateCustomerIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CustomerId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.customers.v1.CustomerService/UpdateCustomer and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateCustomerId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCustomers.CustomerService.CustomerServiceClient(channel);
        var response = await source.UpdateCustomerAsync(new ApiCustomers.UpdateCustomerRequest(), cancellationToken: token);
        var record = new PropagateCustomerIdRecord
        {
            CustomerId = response.Customer.CustomerId,
            CorrelationId = response.Result.CorrelationId,
            Message = response.Result.Message,
            Status = response.Result.Status,
        };
        await DeliverPropagateCustomerIdAsync("change-request-reporting", record, channel, token);
    }

    private static async Task DeliverPropagateCustomerIdAsync(string kind, PropagateCustomerIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateCustomerIdSink sink = new PropagateCustomerIdLogOnly();
        switch (kind)
        {
            case "course-enrollment-approval":
                sink = new PropagateCustomerIdCourseEnrollmentApprovalWriter(new ApiLearning.CourseEnrollmentApprovalService.CourseEnrollmentApprovalServiceClient(channel));
                break;
            case "building-lifecycle":
                sink = new PropagateCustomerIdBuildingLifecycleRelay(new ApiWorkplaces.BuildingLifecycleService.BuildingLifecycleServiceClient(channel));
                break;
            case "change-request-reporting":
                sink = new PropagateCustomerIdChangeRequestReportingTarget(new ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient(channel));
                break;
            case "feedback-workflow":
                sink = new PropagateCustomerIdFeedbackWorkflowSink(new ApiPerformance.FeedbackWorkflowService.FeedbackWorkflowServiceClient(channel));
                break;
        }
        await sink.WriteAsync(record, token);
    }
}
