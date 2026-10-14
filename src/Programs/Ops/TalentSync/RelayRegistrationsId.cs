using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;

namespace Acme.Workforce.Ops.TalentSync;

/// <summary>Where RelayRegistrationsId hands each record.</summary>
internal interface IRelayRegistrationsIdTarget
{
    Task SendAsync(RelayRegistrationsIdRecord record, CancellationToken token);
}

/// <summary>What RelayRegistrationsId keeps of each CountRegistrations response.</summary>
internal sealed class RelayRegistrationsIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Writes records to acme.devices.v1.ComplianceScanCalculationService.</summary>
internal sealed class RelayRegistrationsIdComplianceScanCalculationSink : IRelayRegistrationsIdTarget
{
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _client;

    public RelayRegistrationsIdComplianceScanCalculationSink(ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient client) => _client = client;

    public async Task SendAsync(RelayRegistrationsIdRecord record, CancellationToken token)
    {
        await _client.CalculateComplianceScanAsync(new ApiDevices.CalculateComplianceScanRequest { ComplianceScanId = record.Status, ComplianceScan = new ApiDevices.ComplianceScan { Kind = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.identity.v1.UserService.</summary>
internal sealed class RelayRegistrationsIdUserTarget : IRelayRegistrationsIdTarget
{
    private readonly ApiIdentity.UserService.UserServiceClient _client;

    public RelayRegistrationsIdUserTarget(ApiIdentity.UserService.UserServiceClient client) => _client = client;

    public async Task SendAsync(RelayRegistrationsIdRecord record, CancellationToken token)
    {
        await _client.ListUsersAsync(new ApiIdentity.ListUsersRequest { Filter = new ApiIdentity.ListUsersRequest.Types.Filter { OwnerId = record.Status }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.learning.v1.CourseEnrollmentLifecycleService.</summary>
internal sealed class RelayRegistrationsIdCourseEnrollmentLifecycleSink(ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient client) : IRelayRegistrationsIdTarget
{
    public async Task SendAsync(RelayRegistrationsIdRecord record, CancellationToken token)
    {
        await client.TerminateCourseEnrollmentAsync(new ApiLearning.TerminateCourseEnrollmentRequest { Reason = record.Status }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class RelayRegistrationsIdLogOnly : IRelayRegistrationsIdTarget
{
    public Task SendAsync(RelayRegistrationsIdRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.CorrelationId}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.corporateevents.v1.RegistrationQueryService/CountRegistrations and hands what it keeps to the sink of its kind.</summary>
internal static class RelayRegistrationsId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient(channel);
        var reply = await source.CountRegistrationsAsync(new ApiCorporateevents.CountRegistrationsRequest() { OrderBy = "main-relay-registrations-id" }, cancellationToken: token);
        var record = new RelayRegistrationsIdRecord
        {
            CorrelationId = Text.Normalize(reply.Result.CorrelationId),
            Status = reply.Result.Status,
        };
        await DeliverRelayRegistrationsIdAsync("user", record, channel, token);
    }

    private static async Task DeliverRelayRegistrationsIdAsync(string kind, RelayRegistrationsIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        IRelayRegistrationsIdTarget sink = kind switch
        {
            "compliance-scan-calculation" => new RelayRegistrationsIdComplianceScanCalculationSink(new ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient(channel)),
            "user" => new RelayRegistrationsIdUserTarget(new ApiIdentity.UserService.UserServiceClient(channel)),
            "course-enrollment-lifecycle" => new RelayRegistrationsIdCourseEnrollmentLifecycleSink(new ApiLearning.CourseEnrollmentLifecycleService.CourseEnrollmentLifecycleServiceClient(channel)),
            _ => new RelayRegistrationsIdLogOnly(),
        };
        await sink.SendAsync(record, token);
    }
}
