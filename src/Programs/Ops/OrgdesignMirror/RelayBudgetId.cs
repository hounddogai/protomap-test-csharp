using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBudgeting = Acme.Budgeting.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiPrescriptions = Acme.Prescriptions.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.OrgdesignMirror;

/// <summary>Where RelayBudgetId hands each record.</summary>
internal interface IRelayBudgetIdWriter
{
    Task PushAsync(RelayBudgetIdRecord record, CancellationToken token);
}

/// <summary>What RelayBudgetId keeps of each QuoteBudget response.</summary>
internal sealed class RelayBudgetIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string BudgetId { get; init; } = "";
}

/// <summary>Writes records to acme.documents.v1.DocumentService.</summary>
internal sealed class RelayBudgetIdDocumentWriter(ApiDocuments.DocumentService.DocumentServiceClient client) : IRelayBudgetIdWriter
{
    public async Task PushAsync(RelayBudgetIdRecord record, CancellationToken token)
    {
        await client.DeleteDocumentAsync(new ApiDocuments.DeleteDocumentRequest { DocumentId = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.recruiting.v1.InterviewValidationService.</summary>
internal sealed class RelayBudgetIdInterviewValidationTarget : IRelayBudgetIdWriter
{
    private readonly ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient _client;

    public RelayBudgetIdInterviewValidationTarget(ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient client) => _client = client;

    public async Task PushAsync(RelayBudgetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.ValidateInterviewRequest { Reason = record.BudgetId, Interview = new ApiRecruiting.Interview { Title = record.CorrelationId } }, token, _client.ValidateInterviewAsync);
    }
}

/// <summary>Writes records to acme.employeeportal.v1.SelfServiceRequestIntegrationService.</summary>
internal sealed class RelayBudgetIdSelfServiceRequestIntegrationTarget : IRelayBudgetIdWriter
{
    private readonly ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient _client;

    public RelayBudgetIdSelfServiceRequestIntegrationTarget(ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient client) => _client = client;

    public async Task PushAsync(RelayBudgetIdRecord record, CancellationToken token)
    {
        await _client.ReplaySelfServiceRequestEventsAsync(new ApiEmployeeportal.ReplaySelfServiceRequestEventsRequest { Reason = record.BudgetId, Details = new ApiEmployeeportal.ReplaySelfServiceRequestEventsRequest.Types.Details { SourceSystem = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.prescriptions.v1.PrescriptionCalculationService.</summary>
internal sealed class RelayBudgetIdPrescriptionCalculationRelay(ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient client) : IRelayBudgetIdWriter
{
    public async Task PushAsync(RelayBudgetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiPrescriptions.ExplainPrescriptionCalculationRequest { Prescription = new ApiPrescriptions.Prescription { PrescriptionId = record.BudgetId, Number = record.CorrelationId } }, token, client.ExplainPrescriptionCalculationAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayBudgetIdLogOnly : IRelayBudgetIdWriter
{
    public Task PushAsync(RelayBudgetIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.budgeting.v1.BudgetCalculationService/QuoteBudget and hands what it keeps to the sink of its kind.</summary>
internal static class RelayBudgetId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBudgeting.BudgetCalculationService.BudgetCalculationServiceClient(channel);
        var got = await source.QuoteBudgetAsync(new ApiBudgeting.QuoteBudgetRequest() { BudgetId = "primary-relay-budget-id" }, cancellationToken: token);
        var record = new RelayBudgetIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            BudgetId = got.Budget.BudgetId,
        };
        await DeliverRelayBudgetIdAsync("document", record, channel, token);
    }

    private static async Task DeliverRelayBudgetIdAsync(string kind, RelayBudgetIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayBudgetIdWriter sink = new RelayBudgetIdLogOnly();
        switch (kind)
        {
            case "document":
                sink = new RelayBudgetIdDocumentWriter(new ApiDocuments.DocumentService.DocumentServiceClient(channel));
                break;
            case "interview-validation":
                sink = new RelayBudgetIdInterviewValidationTarget(new ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient(channel));
                break;
            case "self-request-integration":
                sink = new RelayBudgetIdSelfServiceRequestIntegrationTarget(new ApiEmployeeportal.SelfServiceRequestIntegrationService.SelfServiceRequestIntegrationServiceClient(channel));
                break;
            case "prescription-calculation":
                sink = new RelayBudgetIdPrescriptionCalculationRelay(new ApiPrescriptions.PrescriptionCalculationService.PrescriptionCalculationServiceClient(channel));
                break;
        }
        await sink.PushAsync(record, token);
    }
}
