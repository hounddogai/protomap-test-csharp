using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Ops.ContractorsAuditor;

/// <summary>Where MirrorCorporateEventApprovalId hands each record.</summary>
internal interface IMirrorCorporateEventApprovalIdWriter
{
    Task ApplyAsync(MirrorCorporateEventApprovalIdRecord record, CancellationToken token);
}

/// <summary>What MirrorCorporateEventApprovalId keeps of each DelegateCorporateEventApproval response.</summary>
internal sealed class MirrorCorporateEventApprovalIdRecord
{
    public string ReferenceId { get; init; } = "";
    public string Message { get; init; } = "";
    public string Title { get; init; } = "";
}

/// <summary>Hands records to acme.yards.v1.YardService.</summary>
internal sealed class MirrorCorporateEventApprovalIdYardSink(ApiYards.YardService.YardServiceClient client) : IMirrorCorporateEventApprovalIdWriter
{
    public async Task ApplyAsync(MirrorCorporateEventApprovalIdRecord record, CancellationToken token)
    {
        await client.SearchYardsAsync(new ApiYards.SearchYardsRequest { Filter = new ApiYards.SearchYardsRequest.Types.Filter { Query = record.Message, Region = record.Title }, Page = new ApiCommon.PageRequest { PageToken = record.ReferenceId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.onboarding.v1.OnboardingTaskReportingService.</summary>
internal sealed class MirrorCorporateEventApprovalIdOnboardingTaskReportingTarget(ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient client) : IMirrorCorporateEventApprovalIdWriter
{
    public async Task ApplyAsync(MirrorCorporateEventApprovalIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiOnboarding.GenerateOnboardingTaskReportRequest { Reason = record.Message }, token, client.GenerateOnboardingTaskReportAsync);
    }
}

/// <summary>Reads acme.corporateevents.v1.CorporateEventApprovalService/DelegateCorporateEventApproval and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorCorporateEventApprovalId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient(channel);
        var got = await source.DelegateCorporateEventApprovalAsync(new ApiCorporateevents.DelegateCorporateEventApprovalRequest() { Reason = "live-mirror-corporate-event-approval-id" }, cancellationToken: token);
        var record = new MirrorCorporateEventApprovalIdRecord
        {
            ReferenceId = got.Outcome.ReferenceId,
            Message = got.Outcome.Message,
            Title = got.CorporateEvent.Title,
        };
        var sinks = new List<IMirrorCorporateEventApprovalIdWriter>
        {
            new MirrorCorporateEventApprovalIdYardSink(new ApiYards.YardService.YardServiceClient(channel)),
            new MirrorCorporateEventApprovalIdOnboardingTaskReportingTarget(new ApiOnboarding.OnboardingTaskReportingService.OnboardingTaskReportingServiceClient(channel)),
        };
        await sinks[1].ApplyAsync(record, token);
    }
}
