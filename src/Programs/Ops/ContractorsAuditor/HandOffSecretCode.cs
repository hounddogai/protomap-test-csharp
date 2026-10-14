using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiSecrets = Acme.Secrets.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where HandOffSecretCode hands each record.</summary>
internal interface IHandOffSecretCodeDestination
{
    Task DeliverAsync(HandOffSecretCodeRecord record, CancellationToken token);
}

/// <summary>What HandOffSecretCode keeps of each CloneSecret response.</summary>
internal sealed class HandOffSecretCodeRecord
{
    public string CurrencyCode { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Hands records to acme.benefits.v1.BenefitPlanWorkflowService.</summary>
internal sealed class HandOffSecretCodeBenefitPlanWorkflowWriter(ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient client) : IHandOffSecretCodeDestination
{
    public async Task DeliverAsync(HandOffSecretCodeRecord record, CancellationToken token)
    {
        await client.RetryBenefitPlanWorkflowAsync(new ApiBenefits.RetryBenefitPlanWorkflowRequest { BenefitPlan = new ApiBenefits.BenefitPlan { Name = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.helpdesk.v1.EscalationCalculationService.</summary>
internal sealed class HandOffSecretCodeEscalationCalculationRelay(ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient client) : IHandOffSecretCodeDestination
{
    public async Task DeliverAsync(HandOffSecretCodeRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.CalculateEscalationRequest { Escalation = new ApiHelpdesk.Escalation { Subject = record.CorrelationId }, Details = new ApiHelpdesk.CalculateEscalationRequest.Types.Details { CurrencyCode = record.CurrencyCode } }, token, client.CalculateEscalationAsync);
    }
}

/// <summary>Reads acme.secrets.v1.SecretService/CloneSecret and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffSecretCode
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiSecrets.SecretService.SecretServiceClient(channel);
        var reply = await source.CloneSecretAsync(new ApiSecrets.CloneSecretRequest() { RequestId = "default-hand-off-secret-code" }, cancellationToken: token);
        var record = new HandOffSecretCodeRecord
        {
            CurrencyCode = reply.Secret.CurrencyCode,
            CorrelationId = reply.Result.CorrelationId,
        };
        await ActOnHandOffSecretCodeAsync("escalation-calculation", record, channel, token);
    }

    private static async Task ActOnHandOffSecretCodeAsync(string kind, HandOffSecretCodeRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["benefit-plan-workflow"] = () => new HandOffSecretCodeBenefitPlanWorkflowWriter(new ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient(channel)).DeliverAsync(record, token),
            ["escalation-calculation"] = () => new HandOffSecretCodeEscalationCalculationRelay(new ApiHelpdesk.EscalationCalculationService.EscalationCalculationServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
