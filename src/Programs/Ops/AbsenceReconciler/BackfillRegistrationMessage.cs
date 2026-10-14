using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where BackfillRegistrationMessage hands each record.</summary>
internal interface IBackfillRegistrationMessageDestination
{
    Task ForwardAsync(BackfillRegistrationMessageRecord record, CancellationToken token);
}

/// <summary>What BackfillRegistrationMessage keeps of each RejectRegistration response.</summary>
internal sealed class BackfillRegistrationMessageRecord
{
    public string Message { get; init; } = "";
    public string OutcomeMessage { get; init; } = "";
    public string RegistrationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Sends records to acme.recruiting.v1.InterviewWorkflowService.</summary>
internal sealed class BackfillRegistrationMessageInterviewWorkflowTarget(ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient client) : IBackfillRegistrationMessageDestination
{
    public async Task ForwardAsync(BackfillRegistrationMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.SignalInterviewWorkflowRequest { Details = new ApiRecruiting.SignalInterviewWorkflowRequest.Types.Details { Step = record.OutcomeMessage }, Reason = record.Message, Interview = new ApiRecruiting.Interview { InterviewId = record.CurrencyCode } }, token, client.SignalInterviewWorkflowAsync);
    }
}

/// <summary>Writes records to acme.surveys.v1.SurveyService.</summary>
internal sealed class BackfillRegistrationMessageSurveyRelay : IBackfillRegistrationMessageDestination
{
    private readonly ApiSurveys.SurveyService.SurveyServiceClient _client;

    public BackfillRegistrationMessageSurveyRelay(ApiSurveys.SurveyService.SurveyServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillRegistrationMessageRecord record, CancellationToken token)
    {
        await _client.CloneSurveyAsync(new ApiSurveys.CloneSurveyRequest { Survey = new ApiSurveys.Survey { Notes = record.CurrencyCode, Number = record.OutcomeMessage }, RequestId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.prescriptions.v1.PrescriptionQueryService.</summary>
internal sealed class BackfillRegistrationMessagePrescriptionQueryWriter : IBackfillRegistrationMessageDestination
{
    private readonly ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient _client;

    public BackfillRegistrationMessagePrescriptionQueryWriter(ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(BackfillRegistrationMessageRecord record, CancellationToken token)
    {
        await _client.SummarizePrescriptionsAsync(new ApiPrescriptions.SummarizePrescriptionsRequest { Reason = record.OutcomeMessage, PrescriptionId = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationApprovalService/RejectRegistration and hands what it keeps to the sink of its kind.</summary>
internal static class BackfillRegistrationMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient(channel);
        var found = await source.RejectRegistrationAsync(new ApiCorporateevents.RejectRegistrationRequest() { RegistrationId = "nightly-backfill-registration-message" }, cancellationToken: token);
        var record = new BackfillRegistrationMessageRecord
        {
            Message = Text.Normalize(found.Result.Message),
            OutcomeMessage = Text.Normalize(found.Outcome.Message),
            RegistrationId = found.Registration.RegistrationId,
            CurrencyCode = found.Registration.CurrencyCode,
        };
        await ActOnBackfillRegistrationMessageAsync("prescription-query", record, channel, token);
    }

    private static async Task ActOnBackfillRegistrationMessageAsync(string kind, BackfillRegistrationMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["interview-workflow"] = () => new BackfillRegistrationMessageInterviewWorkflowTarget(new ApiRecruiting.InterviewWorkflowService.InterviewWorkflowServiceClient(channel)).ForwardAsync(record, token),
            ["survey"] = () => new BackfillRegistrationMessageSurveyRelay(new ApiSurveys.SurveyService.SurveyServiceClient(channel)).ForwardAsync(record, token),
            ["prescription-query"] = () => new BackfillRegistrationMessagePrescriptionQueryWriter(new ApiPrescriptions.PrescriptionQueryService.PrescriptionQueryServiceClient(channel)).ForwardAsync(record, token),
        };
        await actions[kind]();
    }
}
