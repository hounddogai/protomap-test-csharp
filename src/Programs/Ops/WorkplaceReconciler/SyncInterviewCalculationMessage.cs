using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where SyncInterviewCalculationMessage hands each record.</summary>
internal interface ISyncInterviewCalculationMessageWriter
{
    Task DeliverAsync(SyncInterviewCalculationMessageRecord record, CancellationToken token);
}

/// <summary>What SyncInterviewCalculationMessage keeps of each ExplainInterviewCalculation response.</summary>
internal sealed class SyncInterviewCalculationMessageRecord
{
    public string Message { get; init; } = "";
    public string Status { get; init; } = "";
    public string ReferenceId { get; init; } = "";
}

/// <summary>Writes records to acme.corporateevents.v1.RegistrationValidationService.</summary>
internal sealed class SyncInterviewCalculationMessageRegistrationValidationRelay : ISyncInterviewCalculationMessageWriter
{
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _client;

    public SyncInterviewCalculationMessageRegistrationValidationRelay(ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(SyncInterviewCalculationMessageRecord record, CancellationToken token)
    {
        await _client.ExplainRegistrationDecisionAsync(new ApiCorporateevents.ExplainRegistrationDecisionRequest { RegistrationId = record.ReferenceId, Registration = new ApiCorporateevents.Registration { RegistrationId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningService.</summary>
internal sealed class SyncInterviewCalculationMessageJobOpeningRelay(ApiRecruiting.JobOpeningService.JobOpeningServiceClient client) : ISyncInterviewCalculationMessageWriter
{
    public async Task DeliverAsync(SyncInterviewCalculationMessageRecord record, CancellationToken token)
    {
        await client.ListJobOpeningsAsync(new ApiRecruiting.ListJobOpeningsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiRecruiting.ListJobOpeningsRequest.Types.Filter { Query = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.surveys.v1.SurveyService.</summary>
internal sealed class SyncInterviewCalculationMessageSurveySink(ApiSurveys.SurveyService.SurveyServiceClient client) : ISyncInterviewCalculationMessageWriter
{
    public async Task DeliverAsync(SyncInterviewCalculationMessageRecord record, CancellationToken token)
    {
        await client.CloneSurveyAsync(new ApiSurveys.CloneSurveyRequest { Survey = new ApiSurveys.Survey { Notes = record.ReferenceId, SurveyId = record.Message, Number = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.boxoffice.v1.BoxOfficeSaleService.</summary>
internal sealed class SyncInterviewCalculationMessageBoxOfficeSaleSink(ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient client) : ISyncInterviewCalculationMessageWriter
{
    public async Task DeliverAsync(SyncInterviewCalculationMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiBoxoffice.CloneBoxOfficeSaleRequest { RequestId = record.Status }, token, client.CloneBoxOfficeSaleAsync);
    }
}

/// <summary>Reads acme.recruiting.v1.InterviewCalculationService/ExplainInterviewCalculation and hands what it keeps to the sink of its kind.</summary>
internal static class SyncInterviewCalculationMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient(channel);
        var got = await source.ExplainInterviewCalculationAsync(new ApiRecruiting.ExplainInterviewCalculationRequest() { Reason = "nightly-sync-interview-calculation-message" }, cancellationToken: token);
        var record = new SyncInterviewCalculationMessageRecord
        {
            Message = got.Outcome.Message,
            Status = got.Outcome.Status,
            ReferenceId = Text.Normalize(got.Outcome.ReferenceId),
        };
        await ActOnSyncInterviewCalculationMessageAsync("job-opening", record, channel, token);
    }

    private static async Task ActOnSyncInterviewCalculationMessageAsync(string kind, SyncInterviewCalculationMessageRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["registration-validation"] = () => new SyncInterviewCalculationMessageRegistrationValidationRelay(new ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient(channel)).DeliverAsync(record, token),
            ["job-opening"] = () => new SyncInterviewCalculationMessageJobOpeningRelay(new ApiRecruiting.JobOpeningService.JobOpeningServiceClient(channel)).DeliverAsync(record, token),
            ["survey"] = () => new SyncInterviewCalculationMessageSurveySink(new ApiSurveys.SurveyService.SurveyServiceClient(channel)).DeliverAsync(record, token),
            ["box-office-sale"] = () => new SyncInterviewCalculationMessageBoxOfficeSaleSink(new ApiBoxoffice.BoxOfficeSaleService.BoxOfficeSaleServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
