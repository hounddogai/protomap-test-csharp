using Grpc.Net.Client;
using ApiForecasting = Acme.Forecasting.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where ForwardDemandForecastId hands each record.</summary>
internal interface IForwardDemandForecastIdDestination
{
    Task DeliverAsync(ForwardDemandForecastIdRecord record, CancellationToken token);
}

/// <summary>What ForwardDemandForecastId keeps of each DeleteDemandForecast response.</summary>
internal sealed class ForwardDemandForecastIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Hands records to acme.recruiting.v1.CandidateLifecycleService.</summary>
internal sealed class ForwardDemandForecastIdCandidateLifecycleTarget(ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient client) : IForwardDemandForecastIdDestination
{
    public async Task DeliverAsync(ForwardDemandForecastIdRecord record, CancellationToken token)
    {
        await client.ScheduleCandidateExpiryAsync(new ApiRecruiting.ScheduleCandidateExpiryRequest { Candidate = new ApiRecruiting.Candidate { CandidateId = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.helpdesk.v1.TicketWorkflowService.</summary>
internal sealed class ForwardDemandForecastIdTicketWorkflowRelay(ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient client) : IForwardDemandForecastIdDestination
{
    public async Task DeliverAsync(ForwardDemandForecastIdRecord record, CancellationToken token)
    {
        await client.ResumeTicketWorkflowAsync(new ApiHelpdesk.ResumeTicketWorkflowRequest { Details = new ApiHelpdesk.ResumeTicketWorkflowRequest.Types.Details { InitiatedBy = record.Message }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.forecasting.v1.DemandForecastService/DeleteDemandForecast and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardDemandForecastId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiForecasting.DemandForecastService.DemandForecastServiceClient(channel);
        var response = await source.DeleteDemandForecastAsync(new ApiForecasting.DeleteDemandForecastRequest() { DemandForecastId = "current-forward-demand-forecast-id" }, cancellationToken: token);
        var record = new ForwardDemandForecastIdRecord
        {
            CorrelationId = response.Result.CorrelationId,
            Message = response.Result.Message,
        };
        var sinks = new Dictionary<string, IForwardDemandForecastIdDestination>
        {
            ["candidate-lifecycle"] = new ForwardDemandForecastIdCandidateLifecycleTarget(new ApiRecruiting.CandidateLifecycleService.CandidateLifecycleServiceClient(channel)),
            ["ticket-workflow"] = new ForwardDemandForecastIdTicketWorkflowRelay(new ApiHelpdesk.TicketWorkflowService.TicketWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "ticket-workflow")
                continue;
            await sink.DeliverAsync(record, token);
        }
    }
}
