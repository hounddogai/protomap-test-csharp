using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiPipelines = Acme.Pipelines.V1;

namespace Acme.Workforce.Ops.PayrollopsRelay;

/// <summary>Where ForwardPipelineId hands each record.</summary>
internal interface IForwardPipelineIdDestination
{
    Task ForwardAsync(ForwardPipelineIdRecord record, CancellationToken token);
}

/// <summary>What ForwardPipelineId keeps of each SimulatePipeline response.</summary>
internal sealed class ForwardPipelineIdRecord
{
    public string PipelineId { get; init; } = "";
    public string Status { get; init; } = "";
    public string Title { get; init; } = "";
}

/// <summary>Writes records to acme.employees.v1.OrgUnitQueryService.</summary>
internal sealed class ForwardPipelineIdOrgUnitQueryRelay : IForwardPipelineIdDestination
{
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _client;

    public ForwardPipelineIdOrgUnitQueryRelay(ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardPipelineIdRecord record, CancellationToken token)
    {
        await _client.LookupOrgUnitAsync(new ApiEmployees.LookupOrgUnitRequest { OrgUnitId = record.Title }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.helpdesk.v1.SlaPolicyCalculationService.</summary>
internal sealed class ForwardPipelineIdSlaPolicyCalculationRelay(ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient client) : IForwardPipelineIdDestination
{
    public async Task ForwardAsync(ForwardPipelineIdRecord record, CancellationToken token)
    {
        await client.ExplainSlaPolicyCalculationAsync(new ApiHelpdesk.ExplainSlaPolicyCalculationRequest { Details = new ApiHelpdesk.ExplainSlaPolicyCalculationRequest.Types.Details { Locale = record.PipelineId }, SlaPolicy = new ApiHelpdesk.SlaPolicy { Name = record.Title } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.benefits.v1.DependentApprovalService.</summary>
internal sealed class ForwardPipelineIdDependentApprovalRelay : IForwardPipelineIdDestination
{
    private readonly ApiBenefits.DependentApprovalService.DependentApprovalServiceClient _client;

    public ForwardPipelineIdDependentApprovalRelay(ApiBenefits.DependentApprovalService.DependentApprovalServiceClient client) => _client = client;

    public async Task ForwardAsync(ForwardPipelineIdRecord record, CancellationToken token)
    {
        await _client.ListPendingDependentsAsync(new ApiBenefits.ListPendingDependentsRequest { Filter = new ApiBenefits.ListPendingDependentsRequest.Types.Filter { Region = record.Status, Query = record.PipelineId } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ForwardPipelineIdLogOnly : IForwardPipelineIdDestination
{
    public Task ForwardAsync(ForwardPipelineIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.PipelineId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.pipelines.v1.PipelineCalculationService/SimulatePipeline and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardPipelineId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient(channel);
        var found = await source.SimulatePipelineAsync(new ApiPipelines.SimulatePipelineRequest() { PipelineId = "live-forward-pipeline-id" }, cancellationToken: token);
        var record = new ForwardPipelineIdRecord
        {
            PipelineId = Text.Normalize(found.Pipeline.PipelineId),
            Status = found.Result.Status,
            Title = found.Pipeline.Title,
        };
        await DeliverForwardPipelineIdAsync("sla-policy-calculation", record, channel, token);
    }

    private static async Task DeliverForwardPipelineIdAsync(string kind, ForwardPipelineIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IForwardPipelineIdDestination sink = new ForwardPipelineIdLogOnly();
        switch (kind)
        {
            case "org-unit-query":
                sink = new ForwardPipelineIdOrgUnitQueryRelay(new ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient(channel));
                break;
            case "sla-policy-calculation":
                sink = new ForwardPipelineIdSlaPolicyCalculationRelay(new ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient(channel));
                break;
            case "dependent-approval":
                sink = new ForwardPipelineIdDependentApprovalRelay(new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(channel));
                break;
        }
        await sink.ForwardAsync(record, token);
    }
}
