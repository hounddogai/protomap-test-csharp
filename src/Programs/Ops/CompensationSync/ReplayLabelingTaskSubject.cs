using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Ops.CompensationSync;

/// <summary>Where ReplayLabelingTaskSubject hands each record.</summary>
internal interface IReplayLabelingTaskSubjectSink
{
    Task SendAsync(ReplayLabelingTaskSubjectRecord record, CancellationToken token);
}

/// <summary>What ReplayLabelingTaskSubject keeps of each CreateLabelingTask response.</summary>
internal sealed class ReplayLabelingTaskSubjectRecord
{
    public string Subject { get; init; } = "";
    public string LabelingTaskId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.timekeeping.v1.OvertimeCalculationService.</summary>
internal sealed class ReplayLabelingTaskSubjectOvertimeCalculationRelay(ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient client) : IReplayLabelingTaskSubjectSink
{
    public async Task SendAsync(ReplayLabelingTaskSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTimekeeping.GetOvertimeRatesRequest { OvertimeId = record.Status }, token, client.GetOvertimeRatesAsync);
    }
}

/// <summary>Hands records to acme.devices.v1.DevicePolicyValidationService.</summary>
internal sealed class ReplayLabelingTaskSubjectDevicePolicyValidationSink(ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient client) : IReplayLabelingTaskSubjectSink
{
    public async Task SendAsync(ReplayLabelingTaskSubjectRecord record, CancellationToken token)
    {
        await client.NormalizeDevicePolicyAsync(new ApiDevices.NormalizeDevicePolicyRequest { DevicePolicy = new ApiDevices.DevicePolicy { Description = record.Subject } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.documents.v1.SignatureAdminService.</summary>
internal sealed class ReplayLabelingTaskSubjectSignatureAdminTarget(ApiDocuments.SignatureAdminService.SignatureAdminServiceClient client) : IReplayLabelingTaskSubjectSink
{
    public async Task SendAsync(ReplayLabelingTaskSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.InspectSignatureRequest { SignatureId = record.LabelingTaskId }, token, client.InspectSignatureAsync);
    }
}

/// <summary>Writes records to acme.documents.v1.DocumentFolderQueryService.</summary>
internal sealed class ReplayLabelingTaskSubjectDocumentFolderQueryWriter(ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient client) : IReplayLabelingTaskSubjectSink
{
    public async Task SendAsync(ReplayLabelingTaskSubjectRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDocuments.BrowseDocumentFoldersRequest { Filter = new ApiDocuments.BrowseDocumentFoldersRequest.Types.Filter { OwnerId = record.Subject, Region = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.LabelingTaskId } }, token, client.BrowseDocumentFoldersAsync);
    }
}

/// <summary>Reads acme.labeling.v1.LabelingTaskService/CreateLabelingTask and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayLabelingTaskSubject
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiLabeling.LabelingTaskService.LabelingTaskServiceClient(channel);
        var reply = await source.CreateLabelingTaskAsync(new ApiLabeling.CreateLabelingTaskRequest() { RequestId = "live-replay-labeling-task-subject" }, cancellationToken: token);
        var record = new ReplayLabelingTaskSubjectRecord
        {
            Subject = reply.LabelingTask.Subject,
            LabelingTaskId = Text.Normalize(reply.LabelingTask.LabelingTaskId),
            Status = reply.Result.Status,
        };
        var sinks = new List<IReplayLabelingTaskSubjectSink>
        {
            new ReplayLabelingTaskSubjectOvertimeCalculationRelay(new ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient(channel)),
            new ReplayLabelingTaskSubjectDevicePolicyValidationSink(new ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient(channel)),
            new ReplayLabelingTaskSubjectSignatureAdminTarget(new ApiDocuments.SignatureAdminService.SignatureAdminServiceClient(channel)),
            new ReplayLabelingTaskSubjectDocumentFolderQueryWriter(new ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient(channel)),
        };
        await sinks[2].SendAsync(record, token);
    }
}
