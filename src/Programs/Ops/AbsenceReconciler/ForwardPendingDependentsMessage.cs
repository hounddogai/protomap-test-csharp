using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>What ForwardPendingDependentsMessage keeps of each ListPendingDependents response.</summary>
internal sealed class ForwardPendingDependentsMessageRecord
{
    public string Message { get; init; } = "";
    public string NextPageToken { get; init; } = "";
}

/// <summary>Hands records to acme.timekeeping.v1.OvertimeLifecycleService.</summary>
internal sealed class ForwardPendingDependentsMessageOvertimeLifecycleSink(ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient client)
{
    public async Task DeliverAsync(ForwardPendingDependentsMessageRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.ReinstateOvertimeRequest { OvertimeId = record.Message, Reason = record.NextPageToken }, token, client.ReinstateOvertimeAsync);
    }
}

/// <summary>Reads acme.benefits.v1.DependentApprovalService/ListPendingDependents and hands what it keeps to acme.timekeeping.v1.OvertimeLifecycleService.</summary>
internal static class ForwardPendingDependentsMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.DependentApprovalService.DependentApprovalServiceClient(channel);
        var got = await source.ListPendingDependentsAsync(new ApiBenefits.ListPendingDependentsRequest() { OrderBy = "current-forward-pending-dependents-message" }, cancellationToken: token);
        var record = new ForwardPendingDependentsMessageRecord
        {
            Message = got.Result.Message,
            NextPageToken = got.PageInfo.NextPageToken,
        };
        await new ForwardPendingDependentsMessageOvertimeLifecycleSink(new ApiTimekeeping.OvertimeLifecycleService.OvertimeLifecycleServiceClient(channel)).DeliverAsync(record, token);
    }
}
