using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiCompliance = Acme.Compliance.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where HandOffBudgetsMessage hands each record.</summary>
internal interface IHandOffBudgetsMessageTarget
{
    Task SendAsync(HandOffBudgetsMessageRecord record, CancellationToken token);
}

/// <summary>What HandOffBudgetsMessage keeps of each CountBudgets response.</summary>
internal sealed class HandOffBudgetsMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.compliance.v1.ComplianceCaseService.</summary>
internal sealed class HandOffBudgetsMessageComplianceCaseWriter(ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient client) : IHandOffBudgetsMessageTarget
{
    public async Task SendAsync(HandOffBudgetsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCompliance.ListComplianceCasesRequest { Filter = new ApiCompliance.ListComplianceCasesRequest.Types.Filter { Region = record.CorrelationId }, OrderBy = record.Status, Page = new ApiCommon.PageRequest { PageToken = record.Message } }, token, client.ListComplianceCasesAsync);
    }
}

/// <summary>Sends records to acme.itsm.v1.IncidentService.</summary>
internal sealed class HandOffBudgetsMessageIncidentWriter(ApiItsm.IncidentService.IncidentServiceClient client) : IHandOffBudgetsMessageTarget
{
    public async Task SendAsync(HandOffBudgetsMessageRecord record, CancellationToken token)
    {
        await client.ListIncidentsAsync(new ApiItsm.ListIncidentsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiItsm.ListIncidentsRequest.Types.Filter { Region = record.Message, Query = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.itsm.v1.ChangeRequestReportingService.</summary>
internal sealed class HandOffBudgetsMessageChangeRequestReportingWriter(ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient client) : IHandOffBudgetsMessageTarget
{
    public async Task SendAsync(HandOffBudgetsMessageRecord record, CancellationToken token)
    {
        await client.ScheduleChangeRequestReportAsync(new ApiItsm.ScheduleChangeRequestReportRequest { Reason = record.Message, ChangeRequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.budgeting.v1.BudgetQueryService/CountBudgets and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffBudgetsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBudgeting.BudgetQueryService.BudgetQueryServiceClient(channel);
        var reply = await source.CountBudgetsAsync(new ApiBudgeting.CountBudgetsRequest() { OrderBy = "default-hand-off-budgets-message" }, cancellationToken: token);
        var record = new HandOffBudgetsMessageRecord
        {
            Message = reply.Result.Message,
            Status = reply.Result.Status,
            CorrelationId = reply.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, IHandOffBudgetsMessageTarget>
        {
            ["compliance-case"] = new HandOffBudgetsMessageComplianceCaseWriter(new ApiCompliance.ComplianceCaseService.ComplianceCaseServiceClient(channel)),
            ["incident"] = new HandOffBudgetsMessageIncidentWriter(new ApiItsm.IncidentService.IncidentServiceClient(channel)),
            ["change-request-reporting"] = new HandOffBudgetsMessageChangeRequestReportingWriter(new ApiItsm.ChangeRequestReportingService.ChangeRequestReportingServiceClient(channel)),
        };
        await RouteHandOffBudgetsMessageAsync("change-request-reporting", record, sinks, token);
    }

    private static async Task RouteHandOffBudgetsMessageAsync(string kind, HandOffBudgetsMessageRecord record, IReadOnlyDictionary<string, IHandOffBudgetsMessageTarget> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.SendAsync(record, token);
    }
}
