using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where ReplayEnrollmentApprovalStatusStatus hands each record.</summary>
internal interface IReplayEnrollmentApprovalStatusStatusDestination
{
    Task ForwardAsync(ReplayEnrollmentApprovalStatusStatusRecord record, CancellationToken token);
}

/// <summary>What ReplayEnrollmentApprovalStatusStatus keeps of each GetEnrollmentApprovalStatus response.</summary>
internal sealed class ReplayEnrollmentApprovalStatusStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
}

/// <summary>Sends records to acme.authorization.v1.AccessPolicyService.</summary>
internal sealed class ReplayEnrollmentApprovalStatusStatusAccessPolicySink : IReplayEnrollmentApprovalStatusStatusDestination
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _client;

    public ReplayEnrollmentApprovalStatusStatusAccessPolicySink(ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayEnrollmentApprovalStatusStatusRecord record, CancellationToken token)
    {
        await _client.CloneAccessPolicyAsync(new ApiAuthorization.CloneAccessPolicyRequest { AccessPolicy = new ApiAuthorization.AccessPolicy { Name = record.CorrelationId, Description = record.CurrencyCode }, RequestId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.recruiting.v1.JobOpeningService.</summary>
internal sealed class ReplayEnrollmentApprovalStatusStatusJobOpeningWriter(ApiRecruiting.JobOpeningService.JobOpeningServiceClient client) : IReplayEnrollmentApprovalStatusStatusDestination
{
    public async Task ForwardAsync(ReplayEnrollmentApprovalStatusStatusRecord record, CancellationToken token)
    {
        await client.ListJobOpeningsAsync(new ApiRecruiting.ListJobOpeningsRequest { OrderBy = record.CorrelationId, Filter = new ApiRecruiting.ListJobOpeningsRequest.Types.Filter { Region = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.devices.v1.ManagedDeviceQueryService.</summary>
internal sealed class ReplayEnrollmentApprovalStatusStatusManagedDeviceQueryTarget : IReplayEnrollmentApprovalStatusStatusDestination
{
    private readonly ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient _client;

    public ReplayEnrollmentApprovalStatusStatusManagedDeviceQueryTarget(ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient client) => _client = client;

    public async Task ForwardAsync(ReplayEnrollmentApprovalStatusStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.BrowseManagedDevicesRequest { Page = new ApiCommon.PageRequest { PageToken = record.CurrencyCode } }, token, _client.BrowseManagedDevicesAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class ReplayEnrollmentApprovalStatusStatusLogOnly : IReplayEnrollmentApprovalStatusStatusDestination
{
    public Task ForwardAsync(ReplayEnrollmentApprovalStatusStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.benefits.v1.EnrollmentApprovalService/GetEnrollmentApprovalStatus and hands what it keeps to the sink of its kind.</summary>
internal static class ReplayEnrollmentApprovalStatusStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(channel);
        var got = await source.GetEnrollmentApprovalStatusAsync(new ApiBenefits.GetEnrollmentApprovalStatusRequest() { EnrollmentId = "primary-replay-enrollment-approval-status-status" }, cancellationToken: token);
        var record = new ReplayEnrollmentApprovalStatusStatusRecord
        {
            Status = Text.Normalize(got.Result.Status),
            CorrelationId = got.Result.CorrelationId,
            CurrencyCode = got.Enrollment.CurrencyCode,
        };
        await DeliverReplayEnrollmentApprovalStatusStatusAsync("access-policy", record, channel, token);
    }

    private static async Task DeliverReplayEnrollmentApprovalStatusStatusAsync(string kind, ReplayEnrollmentApprovalStatusStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IReplayEnrollmentApprovalStatusStatusDestination sink = kind switch
        {
            "access-policy" => new ReplayEnrollmentApprovalStatusStatusAccessPolicySink(new ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient(channel)),
            "job-opening" => new ReplayEnrollmentApprovalStatusStatusJobOpeningWriter(new ApiRecruiting.JobOpeningService.JobOpeningServiceClient(channel)),
            "managed-device-query" => new ReplayEnrollmentApprovalStatusStatusManagedDeviceQueryTarget(new ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceClient(channel)),
            _ => new ReplayEnrollmentApprovalStatusStatusLogOnly(),
        };
        await sink.ForwardAsync(record, token);
    }
}
