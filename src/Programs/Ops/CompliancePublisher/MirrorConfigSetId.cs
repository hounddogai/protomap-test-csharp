using Grpc.Net.Client;
using Acme.Workforce.Platform;
using ApiCart = Acme.Cart.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where MirrorConfigSetId hands each record.</summary>
internal interface IMirrorConfigSetIdDestination
{
    Task PushAsync(MirrorConfigSetIdRecord record, CancellationToken token);
}

/// <summary>What MirrorConfigSetId keeps of each CreateConfigSet response.</summary>
internal sealed class MirrorConfigSetIdRecord
{
    public string ParentId { get; init; } = "";
    public string ConfigSetId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.cart.v1.CartQueryService.</summary>
internal sealed class MirrorConfigSetIdCartQueryRelay : IMirrorConfigSetIdDestination
{
    private readonly ApiCart.CartQueryService.CartQueryServiceClient _client;

    public MirrorConfigSetIdCartQueryRelay(ApiCart.CartQueryService.CartQueryServiceClient client) => _client = client;

    public async Task PushAsync(MirrorConfigSetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiCart.LookupCartRequest { CartId = record.ConfigSetId }, token, _client.LookupCartAsync);
    }
}

/// <summary>Passes records to acme.learning.v1.CourseEnrollmentValidationService.</summary>
internal sealed class MirrorConfigSetIdCourseEnrollmentValidationRelay : IMirrorConfigSetIdDestination
{
    private readonly ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient _client;

    public MirrorConfigSetIdCourseEnrollmentValidationRelay(ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient client) => _client = client;

    public async Task PushAsync(MirrorConfigSetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLearning.ValidateCourseEnrollmentRequest { CourseEnrollmentId = record.ParentId }, token, _client.ValidateCourseEnrollmentAsync);
    }
}

/// <summary>Hands records to acme.scheduling.v1.AvailabilityWindowApprovalService.</summary>
internal sealed class MirrorConfigSetIdAvailabilityWindowApprovalRelay : IMirrorConfigSetIdDestination
{
    private readonly ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient _client;

    public MirrorConfigSetIdAvailabilityWindowApprovalRelay(ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient client) => _client = client;

    public async Task PushAsync(MirrorConfigSetIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiScheduling.DelegateAvailabilityWindowApprovalRequest { AvailabilityWindowId = record.ParentId }, token, _client.DelegateAvailabilityWindowApprovalAsync);
    }
}

/// <summary>Reads acme.configuration.v1.ConfigSetService/CreateConfigSet and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorConfigSetId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiConfiguration.ConfigSetService.ConfigSetServiceClient(channel);
        var found = await source.CreateConfigSetAsync(new ApiConfiguration.CreateConfigSetRequest() { RequestId = "primary-mirror-config-set-id" }, cancellationToken: token);
        var record = new MirrorConfigSetIdRecord
        {
            ParentId = found.ConfigSet.ParentId,
            ConfigSetId = found.ConfigSet.ConfigSetId,
            Message = found.Result.Message,
        };
        var sinks = new IMirrorConfigSetIdDestination[]
        {
            new MirrorConfigSetIdCartQueryRelay(new ApiCart.CartQueryService.CartQueryServiceClient(channel)),
            new MirrorConfigSetIdCourseEnrollmentValidationRelay(new ApiLearning.CourseEnrollmentValidationService.CourseEnrollmentValidationServiceClient(channel)),
            new MirrorConfigSetIdAvailabilityWindowApprovalRelay(new ApiScheduling.AvailabilityWindowApprovalService.AvailabilityWindowApprovalServiceClient(channel)),
        };
        await sinks[2].PushAsync(record, token);
    }
}
