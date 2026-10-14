using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where PropagateAccessPolicyRulesId hands each record.</summary>
internal interface IPropagateAccessPolicyRulesIdSink
{
    Task DeliverAsync(PropagateAccessPolicyRulesIdRecord record, CancellationToken token);
}

/// <summary>What PropagateAccessPolicyRulesId keeps of each GetAccessPolicyRules response.</summary>
internal sealed class PropagateAccessPolicyRulesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string AccessPolicyId { get; init; } = "";
    public string Name { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.careplans.v1.CarePlanQueryService.</summary>
internal sealed class PropagateAccessPolicyRulesIdCarePlanQuerySink(ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient client) : IPropagateAccessPolicyRulesIdSink
{
    public async Task DeliverAsync(PropagateAccessPolicyRulesIdRecord record, CancellationToken token)
    {
        await client.PeekCarePlanAsync(new ApiCareplans.PeekCarePlanRequest { CarePlanId = record.Name }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.learning.v1.CertificationApprovalService.</summary>
internal sealed class PropagateAccessPolicyRulesIdCertificationApprovalTarget : IPropagateAccessPolicyRulesIdSink
{
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _client;

    public PropagateAccessPolicyRulesIdCertificationApprovalTarget(ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient client) => _client = client;

    public async Task DeliverAsync(PropagateAccessPolicyRulesIdRecord record, CancellationToken token)
    {
        await _client.RecallCertificationAsync(new ApiLearning.RecallCertificationRequest { Certification = new ApiLearning.Certification { Notes = record.Status }, Reason = record.AccessPolicyId }, cancellationToken: token);
    }
}

/// <summary>Reads acme.authorization.v1.AccessPolicyValidationService/GetAccessPolicyRules and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateAccessPolicyRulesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient(channel);
        var reply = await source.GetAccessPolicyRulesAsync(new ApiAuthorization.GetAccessPolicyRulesRequest() { AccessPolicyId = "primary-propagate-access-policy-rules-id" }, cancellationToken: token);
        var record = new PropagateAccessPolicyRulesIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            AccessPolicyId = Text.Normalize(reply.AccessPolicy.AccessPolicyId),
            Name = Text.Normalize(reply.AccessPolicy.Name),
            Status = Text.Normalize(reply.Result.Status),
        };
        await ActOnPropagateAccessPolicyRulesIdAsync("care-plan-query", record, channel, token);
    }

    private static async Task ActOnPropagateAccessPolicyRulesIdAsync(string kind, PropagateAccessPolicyRulesIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["care-plan-query"] = () => new PropagateAccessPolicyRulesIdCarePlanQuerySink(new ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient(channel)).DeliverAsync(record, token),
            ["certification-approval"] = () => new PropagateAccessPolicyRulesIdCertificationApprovalTarget(new ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient(channel)).DeliverAsync(record, token),
        };
        await actions[kind]();
    }
}
