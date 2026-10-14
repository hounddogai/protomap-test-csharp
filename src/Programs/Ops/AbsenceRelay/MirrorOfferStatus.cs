using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where MirrorOfferStatus hands each record.</summary>
internal interface IMirrorOfferStatusDestination
{
    Task WriteAsync(MirrorOfferStatusRecord record, CancellationToken token);
}

/// <summary>What MirrorOfferStatus keeps of each RenewOffer response.</summary>
internal sealed class MirrorOfferStatusRecord
{
    public string Status { get; init; } = "";
    public string OfferId { get; init; } = "";
}

/// <summary>Passes records to acme.employees.v1.PositionValidationService.</summary>
internal sealed class MirrorOfferStatusPositionValidationRelay(ApiEmployees.PositionValidationService.PositionValidationServiceClient client) : IMirrorOfferStatusDestination
{
    public async Task WriteAsync(MirrorOfferStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployees.LintPositionsRequest { Position = new ApiEmployees.Position { Name = record.OfferId } }, token, client.LintPositionsAsync);
    }
}

/// <summary>Hands records to acme.licensing.v1.SoftwareLicenseApprovalService.</summary>
internal sealed class MirrorOfferStatusSoftwareLicenseApprovalWriter : IMirrorOfferStatusDestination
{
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _client;

    public MirrorOfferStatusSoftwareLicenseApprovalWriter(ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient client) => _client = client;

    public async Task WriteAsync(MirrorOfferStatusRecord record, CancellationToken token)
    {
        await _client.ListPendingSoftwareLicensesAsync(new ApiLicensing.ListPendingSoftwareLicensesRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, OrderBy = record.OfferId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelPolicySyncService.</summary>
internal sealed class MirrorOfferStatusTravelPolicySyncSink : IMirrorOfferStatusDestination
{
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _client;

    public MirrorOfferStatusTravelPolicySyncSink(ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient client) => _client = client;

    public async Task WriteAsync(MirrorOfferStatusRecord record, CancellationToken token)
    {
        await _client.GetTravelPolicySyncStateAsync(new ApiTravel.GetTravelPolicySyncStateRequest { TravelPolicyId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.recruiting.v1.OfferApprovalService.</summary>
internal sealed class MirrorOfferStatusOfferApprovalRelay(ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient client) : IMirrorOfferStatusDestination
{
    public async Task WriteAsync(MirrorOfferStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiRecruiting.RecallOfferRequest { Details = new ApiRecruiting.RecallOfferRequest.Types.Details { Comment = record.Status } }, token, client.RecallOfferAsync);
    }
}

/// <summary>Reads acme.recruiting.v1.OfferLifecycleService/RenewOffer and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorOfferStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceClient(channel);
        var got = await source.RenewOfferAsync(new ApiRecruiting.RenewOfferRequest() { Reason = "default-mirror-offer-status" }, cancellationToken: token);
        var record = new MirrorOfferStatusRecord
        {
            Status = got.Result.Status,
            OfferId = Text.Normalize(got.Offer.OfferId),
        };
        await ActOnMirrorOfferStatusAsync("software-license-approval", record, channel, token);
    }

    private static async Task ActOnMirrorOfferStatusAsync(string kind, MirrorOfferStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["position-validation"] = () => new MirrorOfferStatusPositionValidationRelay(new ApiEmployees.PositionValidationService.PositionValidationServiceClient(channel)).WriteAsync(record, token),
            ["software-license-approval"] = () => new MirrorOfferStatusSoftwareLicenseApprovalWriter(new ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient(channel)).WriteAsync(record, token),
            ["travel-policy-sync"] = () => new MirrorOfferStatusTravelPolicySyncSink(new ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient(channel)).WriteAsync(record, token),
            ["offer-approval"] = () => new MirrorOfferStatusOfferApprovalRelay(new ApiRecruiting.OfferApprovalService.OfferApprovalServiceClient(channel)).WriteAsync(record, token),
        };
        await actions[kind]();
    }
}
