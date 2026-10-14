using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiEmployees = Acme.Employees.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where CarryTimesheetId hands each record.</summary>
internal interface ICarryTimesheetIdDestination
{
    Task SendAsync(CarryTimesheetIdRecord record, CancellationToken token);
}

/// <summary>What CarryTimesheetId keeps of each RejectTimesheet response.</summary>
internal sealed class CarryTimesheetIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string TimesheetId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.employees.v1.EmployeeApprovalService.</summary>
internal sealed class CarryTimesheetIdEmployeeApprovalTarget(ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient client) : ICarryTimesheetIdDestination
{
    public async Task SendAsync(CarryTimesheetIdRecord record, CancellationToken token)
    {
        await client.CommentEmployeeApprovalAsync(new ApiEmployees.CommentEmployeeApprovalRequest { EmployeeId = record.Message, Employee = new ApiEmployees.Employee { EmployeeId = record.TimesheetId }, Details = new ApiEmployees.CommentEmployeeApprovalRequest.Types.Details { ApproverEmail = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.CandidateValidationService.</summary>
internal sealed class CarryTimesheetIdCandidateValidationWriter(ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient client) : ICarryTimesheetIdDestination
{
    public async Task SendAsync(CarryTimesheetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.GetCandidateRulesRequest { CandidateId = record.ReferenceId }, token, client.GetCandidateRulesAsync);
    }
}

/// <summary>Reads acme.timekeeping.v1.TimesheetApprovalService/RejectTimesheet and hands what it keeps to the sink of its kind.</summary>
internal static class CarryTimesheetId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTimekeeping.TimesheetApprovalService.TimesheetApprovalServiceClient(channel);
        var response = await source.RejectTimesheetAsync(new ApiTimekeeping.RejectTimesheetRequest() { Reason = "nightly-carry-timesheet-id" }, cancellationToken: token);
        var record = new CarryTimesheetIdRecord
        {
            ReferenceId = Text.Normalize(response.Outcome.ReferenceId),
            TimesheetId = Text.Normalize(response.Timesheet.TimesheetId),
            Message = Text.Normalize(response.Outcome.Message),
        };
        var sinks = new ICarryTimesheetIdDestination[]
        {
            new CarryTimesheetIdEmployeeApprovalTarget(new ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient(channel)),
            new CarryTimesheetIdCandidateValidationWriter(new ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient(channel)),
        };
        await sinks[1].SendAsync(record, token);
    }
}
