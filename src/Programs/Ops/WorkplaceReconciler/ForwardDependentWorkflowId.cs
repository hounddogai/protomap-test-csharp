using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiBenefits = Acme.Benefits.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.WorkplaceReconciler;

/// <summary>Where ForwardDependentWorkflowId hands each record.</summary>
internal interface IForwardDependentWorkflowIdTarget
{
    Task PushAsync(ForwardDependentWorkflowIdRecord record, CancellationToken token);
}

/// <summary>What ForwardDependentWorkflowId keeps of each ResumeDependentWorkflow response.</summary>
internal sealed class ForwardDependentWorkflowIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.licensing.v1.LicenseAssignmentReportingService.</summary>
internal sealed class ForwardDependentWorkflowIdLicenseAssignmentReportingRelay(ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient client) : IForwardDependentWorkflowIdTarget
{
    public async Task PushAsync(ForwardDependentWorkflowIdRecord record, CancellationToken token)
    {
        await client.ScheduleLicenseAssignmentReportAsync(new ApiLicensing.ScheduleLicenseAssignmentReportRequest { Reason = record.CorrelationId, Details = new ApiLicensing.ScheduleLicenseAssignmentReportRequest.Types.Details { Cron = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.employeeportal.v1.AnnouncementNotificationService.</summary>
internal sealed class ForwardDependentWorkflowIdAnnouncementNotificationWriter(ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient client) : IForwardDependentWorkflowIdTarget
{
    public async Task PushAsync(ForwardDependentWorkflowIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.ListAnnouncementSubscriptionsRequest { Filter = new ApiEmployeeportal.ListAnnouncementSubscriptionsRequest.Types.Filter { Region = record.CorrelationId } }, token, client.ListAnnouncementSubscriptionsAsync);
    }
}

/// <summary>Sends records to acme.onboarding.v1.EquipmentReportingService.</summary>
internal sealed class ForwardDependentWorkflowIdEquipmentReportingSink(ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient client) : IForwardDependentWorkflowIdTarget
{
    public async Task PushAsync(ForwardDependentWorkflowIdRecord record, CancellationToken token)
    {
        await client.CountEquipmentTotalsAsync(new ApiOnboarding.CountEquipmentTotalsRequest { Filter = new ApiOnboarding.CountEquipmentTotalsRequest.Types.Filter { Query = record.CorrelationId, OwnerId = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.benefits.v1.DependentWorkflowService/ResumeDependentWorkflow and hands what it keeps to the sink of its kind.</summary>
internal static class ForwardDependentWorkflowId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.DependentWorkflowService.DependentWorkflowServiceClient(channel);
        var reply = await source.ResumeDependentWorkflowAsync(new ApiBenefits.ResumeDependentWorkflowRequest() { DependentId = "default-forward-dependent-workflow-id" }, cancellationToken: token);
        var record = new ForwardDependentWorkflowIdRecord
        {
            CorrelationId = reply.Result.CorrelationId,
            Status = reply.Outcome.Status,
        };
        var sinks = new Dictionary<string, IForwardDependentWorkflowIdTarget>
        {
            ["license-assignment-reporting"] = new ForwardDependentWorkflowIdLicenseAssignmentReportingRelay(new ApiLicensing.LicenseAssignmentReportingService.LicenseAssignmentReportingServiceClient(channel)),
            ["announcement-notification"] = new ForwardDependentWorkflowIdAnnouncementNotificationWriter(new ApiEmployeeportal.AnnouncementNotificationService.AnnouncementNotificationServiceClient(channel)),
            ["equipment-reporting"] = new ForwardDependentWorkflowIdEquipmentReportingSink(new ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "license-assignment-reporting")
                continue;
            await sink.PushAsync(record, token);
        }
    }
}
