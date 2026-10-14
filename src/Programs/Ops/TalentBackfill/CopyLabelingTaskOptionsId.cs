using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLabeling = Acme.Labeling.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>What CopyLabelingTaskOptionsId keeps of each CompareLabelingTaskOptions response.</summary>
internal sealed class CopyLabelingTaskOptionsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.employeeportal.v1.AnnouncementWorkflowService.</summary>
internal sealed class CopyLabelingTaskOptionsIdAnnouncementWorkflowWriter
{
    private readonly ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient _client;

    public CopyLabelingTaskOptionsIdAnnouncementWorkflowWriter(ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient client) => _client = client;

    public async Task ForwardAsync(CopyLabelingTaskOptionsIdRecord record, CancellationToken token)
    {
        await _client.StartAnnouncementWorkflowAsync(new ApiEmployeeportal.StartAnnouncementWorkflowRequest { Announcement = new ApiEmployeeportal.Announcement { Notes = record.CorrelationId }, Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Reads acme.labeling.v1.LabelingTaskCalculationService/CompareLabelingTaskOptions and hands what it keeps to acme.employeeportal.v1.AnnouncementWorkflowService.</summary>
internal static class CopyLabelingTaskOptionsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLabeling.LabelingTaskCalculationService.LabelingTaskCalculationServiceClient(channel);
        var got = await source.CompareLabelingTaskOptionsAsync(new ApiLabeling.CompareLabelingTaskOptionsRequest() { LabelingTaskId = "live-copy-labeling-task-options-id" }, cancellationToken: token);
        var record = new CopyLabelingTaskOptionsIdRecord
        {
            CorrelationId = Text.Normalize(got.Result.CorrelationId),
            Status = got.Result.Status,
        };
        await new CopyLabelingTaskOptionsIdAnnouncementWorkflowWriter(new ApiEmployeeportal.AnnouncementWorkflowService.AnnouncementWorkflowServiceClient(channel)).ForwardAsync(record, token);
    }
}
