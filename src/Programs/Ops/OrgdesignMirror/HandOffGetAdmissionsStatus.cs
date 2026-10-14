using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiAdmissions = Acme.Admissions.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiPerformance = Acme.Performance.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where HandOffGetAdmissionsStatus hands each record.</summary>
internal interface IHandOffGetAdmissionsStatusSink
{
    Task WriteAsync(HandOffGetAdmissionsStatusRecord record, CancellationToken token);
}

/// <summary>What HandOffGetAdmissionsStatus keeps of each BatchGetAdmissions response.</summary>
internal sealed class HandOffGetAdmissionsStatusRecord
{
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.performance.v1.GoalApprovalService.</summary>
internal sealed class HandOffGetAdmissionsStatusGoalApprovalWriter : IHandOffGetAdmissionsStatusSink
{
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _client;

    public HandOffGetAdmissionsStatusGoalApprovalWriter(ApiPerformance.GoalApprovalService.GoalApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffGetAdmissionsStatusRecord record, CancellationToken token)
    {
        await _client.DelegateGoalApprovalAsync(new ApiPerformance.DelegateGoalApprovalRequest { Details = new ApiPerformance.DelegateGoalApprovalRequest.Types.Details { ApproverEmail = record.Message }, Goal = new ApiPerformance.Goal { Subject = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.experiments.v1.ExperimentService.</summary>
internal sealed class HandOffGetAdmissionsStatusExperimentWriter : IHandOffGetAdmissionsStatusSink
{
    private readonly ApiExperiments.ExperimentService.ExperimentServiceClient _client;

    public HandOffGetAdmissionsStatusExperimentWriter(ApiExperiments.ExperimentService.ExperimentServiceClient client) => _client = client;

    public async Task WriteAsync(HandOffGetAdmissionsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiExperiments.GetExperimentRequest { ExperimentId = record.Message }, token, _client.GetExperimentAsync);
    }
}

/// <summary>Passes records to acme.employees.v1.EmployeeService.</summary>
internal sealed class HandOffGetAdmissionsStatusEmployeeWriter(ApiEmployees.EmployeeService.EmployeeServiceClient client) : IHandOffGetAdmissionsStatusSink
{
    public async Task WriteAsync(HandOffGetAdmissionsStatusRecord record, CancellationToken token)
    {
        await client.CloneEmployeeAsync(new ApiEmployees.CloneEmployeeRequest { RequestId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class HandOffGetAdmissionsStatusLogOnly : IHandOffGetAdmissionsStatusSink
{
    public Task WriteAsync(HandOffGetAdmissionsStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.admissions.v1.AdmissionService/BatchGetAdmissions and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffGetAdmissionsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAdmissions.AdmissionService.AdmissionServiceClient(channel);
        var found = await source.BatchGetAdmissionsAsync(new ApiAdmissions.BatchGetAdmissionsRequest(), cancellationToken: token);
        var record = new HandOffGetAdmissionsStatusRecord
        {
            Status = found.Result.Status,
            Message = found.Result.Message,
        };
        await DeliverHandOffGetAdmissionsStatusAsync("experiment", record, channel, token);
    }

    private static async Task DeliverHandOffGetAdmissionsStatusAsync(string kind, HandOffGetAdmissionsStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IHandOffGetAdmissionsStatusSink sink = new HandOffGetAdmissionsStatusLogOnly();
        switch (kind)
        {
            case "goal-approval":
                sink = new HandOffGetAdmissionsStatusGoalApprovalWriter(new ApiPerformance.GoalApprovalService.GoalApprovalServiceClient(channel));
                break;
            case "experiment":
                sink = new HandOffGetAdmissionsStatusExperimentWriter(new ApiExperiments.ExperimentService.ExperimentServiceClient(channel));
                break;
            case "employee":
                sink = new HandOffGetAdmissionsStatusEmployeeWriter(new ApiEmployees.EmployeeService.EmployeeServiceClient(channel));
                break;
        }
        await sink.WriteAsync(record, token);
    }
}
