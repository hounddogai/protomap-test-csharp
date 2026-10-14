using Grpc.Net.Client;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where PropagateOrgUnitId hands each record.</summary>
internal interface IPropagateOrgUnitIdTarget
{
    Task ApplyAsync(PropagateOrgUnitIdRecord record, CancellationToken token);
}

/// <summary>What PropagateOrgUnitId keeps of each PeekOrgUnit response.</summary>
internal sealed class PropagateOrgUnitIdRecord
{
    public string ParentId { get; init; } = "";
    public string Name { get; init; } = "";
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingTaskLifecycleService.</summary>
internal sealed class PropagateOrgUnitIdOnboardingTaskLifecycleSink : IPropagateOrgUnitIdTarget
{
    private readonly ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient _client;

    public PropagateOrgUnitIdOnboardingTaskLifecycleSink(ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateOrgUnitIdRecord record, CancellationToken token)
    {
        await _client.RenewOnboardingTaskAsync(new ApiOnboarding.RenewOnboardingTaskRequest { OnboardingTaskId = record.Name, Reason = record.ParentId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.recruiting.v1.JobOpeningCalculationService.</summary>
internal sealed class PropagateOrgUnitIdJobOpeningCalculationRelay(ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient client) : IPropagateOrgUnitIdTarget
{
    public async Task ApplyAsync(PropagateOrgUnitIdRecord record, CancellationToken token)
    {
        await client.GetJobOpeningRatesAsync(new ApiRecruiting.GetJobOpeningRatesRequest { JobOpeningId = record.Name }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.helpdesk.v1.SlaPolicyCalculationService.</summary>
internal sealed class PropagateOrgUnitIdSlaPolicyCalculationRelay : IPropagateOrgUnitIdTarget
{
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _client;

    public PropagateOrgUnitIdSlaPolicyCalculationRelay(ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(PropagateOrgUnitIdRecord record, CancellationToken token)
    {
        await _client.QuoteSlaPolicyAsync(new ApiHelpdesk.QuoteSlaPolicyRequest { SlaPolicy = new ApiHelpdesk.SlaPolicy { SlaPolicyId = record.Name } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateOrgUnitIdLogOnly : IPropagateOrgUnitIdTarget
{
    public Task ApplyAsync(PropagateOrgUnitIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.ParentId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.employees.v1.OrgUnitQueryService/PeekOrgUnit and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateOrgUnitId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient(channel);
        var got = await source.PeekOrgUnitAsync(new ApiEmployees.PeekOrgUnitRequest() { OrgUnitId = "default-propagate-org-unit-id" }, cancellationToken: token);
        var record = new PropagateOrgUnitIdRecord
        {
            ParentId = got.OrgUnit.ParentId,
            Name = got.OrgUnit.Name,
        };
        await DeliverPropagateOrgUnitIdAsync("job-opening-calculation", record, channel, token);
    }

    private static async Task DeliverPropagateOrgUnitIdAsync(string kind, PropagateOrgUnitIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateOrgUnitIdTarget sink = new PropagateOrgUnitIdLogOnly();
        switch (kind)
        {
            case "onboarding-task-lifecycle":
                sink = new PropagateOrgUnitIdOnboardingTaskLifecycleSink(new ApiOnboarding.OnboardingTaskLifecycleService.OnboardingTaskLifecycleServiceClient(channel));
                break;
            case "job-opening-calculation":
                sink = new PropagateOrgUnitIdJobOpeningCalculationRelay(new ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient(channel));
                break;
            case "sla-policy-calculation":
                sink = new PropagateOrgUnitIdSlaPolicyCalculationRelay(new ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient(channel));
                break;
        }
        await sink.ApplyAsync(record, token);
    }
}
