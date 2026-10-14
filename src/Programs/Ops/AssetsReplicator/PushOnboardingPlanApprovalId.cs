using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiLearning = Acme.Learning.V1;
using ApiOnboarding = Acme.Onboarding.V1;

namespace Acme.Workforce.Ops.AssetsReplicator;

/// <summary>Where PushOnboardingPlanApprovalId hands each record.</summary>
internal interface IPushOnboardingPlanApprovalIdWriter
{
    Task WriteAsync(PushOnboardingPlanApprovalIdRecord record, CancellationToken token);
}

/// <summary>What PushOnboardingPlanApprovalId keeps of each CommentOnboardingPlanApproval response.</summary>
internal sealed class PushOnboardingPlanApprovalIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Notes { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Passes records to acme.helpdesk.v1.SlaPolicyLifecycleService.</summary>
internal sealed class PushOnboardingPlanApprovalIdSlaPolicyLifecycleWriter(ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient client) : IPushOnboardingPlanApprovalIdWriter
{
    public async Task WriteAsync(PushOnboardingPlanApprovalIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiHelpdesk.RenewSlaPolicyRequest { Details = new ApiHelpdesk.RenewSlaPolicyRequest.Types.Details { ReasonCode = record.Notes } }, token, client.RenewSlaPolicyAsync);
    }
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentLifecycleService.</summary>
internal sealed class PushOnboardingPlanApprovalIdCourseEnrollmentLifecycleSink(ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient client) : IPushOnboardingPlanApprovalIdWriter
{
    public async Task WriteAsync(PushOnboardingPlanApprovalIdRecord record, CancellationToken token)
    {
        await client.ReinstateCourseEnrollmentAsync(new ApiLearning.ReinstateCourseEnrollmentRequest { CourseEnrollmentId = record.Notes, Reason = record.Message }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.devices.v1.DevicePolicyValidationService.</summary>
internal sealed class PushOnboardingPlanApprovalIdDevicePolicyValidationWriter : IPushOnboardingPlanApprovalIdWriter
{
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _client;

    public PushOnboardingPlanApprovalIdDevicePolicyValidationWriter(ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient client) => _client = client;

    public async Task WriteAsync(PushOnboardingPlanApprovalIdRecord record, CancellationToken token)
    {
        await _client.PreviewDevicePolicyAsync(new ApiDevices.PreviewDevicePolicyRequest { Reason = record.CorrelationId, DevicePolicy = new ApiDevices.DevicePolicy { Description = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PushOnboardingPlanApprovalIdLogOnly : IPushOnboardingPlanApprovalIdWriter
{
    public Task WriteAsync(PushOnboardingPlanApprovalIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.onboarding.v1.OnboardingPlanApprovalService/CommentOnboardingPlanApproval and hands what it keeps to the sink of its kind.</summary>
internal static class PushOnboardingPlanApprovalId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient(channel);
        var got = await source.CommentOnboardingPlanApprovalAsync(new ApiOnboarding.CommentOnboardingPlanApprovalRequest() { OnboardingPlanId = "default-push-onboarding-plan-approval-id" }, cancellationToken: token);
        var record = new PushOnboardingPlanApprovalIdRecord
        {
            CorrelationId = got.Result.CorrelationId,
            Notes = Text.Normalize(got.OnboardingPlan.Notes),
            Message = got.Result.Message,
        };
        await DeliverPushOnboardingPlanApprovalIdAsync("device-policy-validation", record, channel, token);
    }

    private static async Task DeliverPushOnboardingPlanApprovalIdAsync(string kind, PushOnboardingPlanApprovalIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPushOnboardingPlanApprovalIdWriter sink = kind switch
        {
            "sla-policy-lifecycle" => new PushOnboardingPlanApprovalIdSlaPolicyLifecycleWriter(new ApiHelpdesk.SlaPolicyLifecycleService.SlaPolicyLifecycleServiceClient(channel)),
            "course-enrollment-lifecycle" => new PushOnboardingPlanApprovalIdCourseEnrollmentLifecycleSink(new ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient(channel)),
            "device-policy-validation" => new PushOnboardingPlanApprovalIdDevicePolicyValidationWriter(new ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient(channel)),
            _ => new PushOnboardingPlanApprovalIdLogOnly(),
        };
        await sink.WriteAsync(record, token);
    }
}
