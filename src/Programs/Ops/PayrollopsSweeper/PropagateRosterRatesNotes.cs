using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where PropagateRosterRatesNotes hands each record.</summary>
internal interface IPropagateRosterRatesNotesDestination
{
    Task SendAsync(PropagateRosterRatesNotesRecord record, CancellationToken token);
}

/// <summary>What PropagateRosterRatesNotes keeps of each GetRosterRates response.</summary>
internal sealed class PropagateRosterRatesNotesRecord
{
    public string Notes { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.learning.v1.LearningPathWorkflowService.</summary>
internal sealed class PropagateRosterRatesNotesLearningPathWorkflowWriter : IPropagateRosterRatesNotesDestination
{
    private readonly ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient _client;

    public PropagateRosterRatesNotesLearningPathWorkflowWriter(ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient client) => _client = client;

    public async Task SendAsync(PropagateRosterRatesNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.InspectLearningPathWorkflowRequest { LearningPathId = record.Status }, token, _client.InspectLearningPathWorkflowAsync);
    }
}

/// <summary>Writes records to acme.helpdesk.v1.SlaPolicyWorkflowService.</summary>
internal sealed class PropagateRosterRatesNotesSlaPolicyWorkflowRelay(ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient client) : IPropagateRosterRatesNotesDestination
{
    public async Task SendAsync(PropagateRosterRatesNotesRecord record, CancellationToken token)
    {
        await client.StartSlaPolicyWorkflowAsync(new ApiHelpdesk.StartSlaPolicyWorkflowRequest { SlaPolicy = new ApiHelpdesk.SlaPolicy { Name = record.Status }, SlaPolicyId = record.Notes }, cancellationToken: token);
    }
}

/// <summary>Reads acme.scheduling.v1.RosterCalculationService/GetRosterRates and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateRosterRatesNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiScheduling.RosterCalculationService.RosterCalculationServiceClient(channel);
        var reply = await source.GetRosterRatesAsync(new ApiScheduling.GetRosterRatesRequest() { RosterId = "main-propagate-roster-rates-notes" }, cancellationToken: token);
        var record = new PropagateRosterRatesNotesRecord
        {
            Notes = reply.Roster.Notes,
            Status = Text.Normalize(reply.Result.Status),
        };
        var sinks = new Dictionary<string, IPropagateRosterRatesNotesDestination>
        {
            ["learning-path-workflow"] = new PropagateRosterRatesNotesLearningPathWorkflowWriter(new ApiLearning.LearningPathWorkflowService.LearningPathWorkflowServiceClient(channel)),
            ["sla-policy-workflow"] = new PropagateRosterRatesNotesSlaPolicyWorkflowRelay(new ApiHelpdesk.SlaPolicyWorkflowService.SlaPolicyWorkflowServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "learning-path-workflow")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
