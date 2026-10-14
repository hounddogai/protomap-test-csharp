using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where HandOffChangeRequestWorkflowsStatus hands each record.</summary>
internal interface IHandOffChangeRequestWorkflowsStatusTarget
{
    Task DeliverAsync(HandOffChangeRequestWorkflowsStatusRecord record, CancellationToken token);
}

/// <summary>What HandOffChangeRequestWorkflowsStatus keeps of each ListChangeRequestWorkflows response.</summary>
internal sealed class HandOffChangeRequestWorkflowsStatusRecord
{
    public string Status { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Writes records to acme.onboarding.v1.EquipmentValidationService.</summary>
internal sealed class HandOffChangeRequestWorkflowsStatusEquipmentValidationTarget : IHandOffChangeRequestWorkflowsStatusTarget
{
    private readonly ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient _client;

    public HandOffChangeRequestWorkflowsStatusEquipmentValidationTarget(ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffChangeRequestWorkflowsStatusRecord record, CancellationToken token)
    {
        await _client.PreviewEquipmentAsync(new ApiOnboarding.PreviewEquipmentRequest { Equipment = new ApiOnboarding.Equipment { EquipmentId = record.Message }, Reason = record.CorrelationId }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelPolicyCalculationService.</summary>
internal sealed class HandOffChangeRequestWorkflowsStatusTravelPolicyCalculationWriter : IHandOffChangeRequestWorkflowsStatusTarget
{
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _client;

    public HandOffChangeRequestWorkflowsStatusTravelPolicyCalculationWriter(ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffChangeRequestWorkflowsStatusRecord record, CancellationToken token)
    {
        await _client.CompareTravelPolicyOptionsAsync(new ApiTravel.CompareTravelPolicyOptionsRequest { TravelPolicy = new ApiTravel.TravelPolicy { Description = record.Message, Name = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.licensing.v1.SoftwareLicenseWorkflowService.</summary>
internal sealed class HandOffChangeRequestWorkflowsStatusSoftwareLicenseWorkflowRelay(ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient client) : IHandOffChangeRequestWorkflowsStatusTarget
{
    public async Task DeliverAsync(HandOffChangeRequestWorkflowsStatusRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiLicensing.PauseSoftwareLicenseWorkflowRequest { Details = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest.Types.Details { InitiatedBy = record.Message } }, token, client.PauseSoftwareLicenseWorkflowAsync);
    }
}

/// <summary>Writes records to acme.telehealth.v1.VideoVisitQueryService.</summary>
internal sealed class HandOffChangeRequestWorkflowsStatusVideoVisitQueryTarget : IHandOffChangeRequestWorkflowsStatusTarget
{
    private readonly ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient _client;

    public HandOffChangeRequestWorkflowsStatusVideoVisitQueryTarget(ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient client) => _client = client;

    public async Task DeliverAsync(HandOffChangeRequestWorkflowsStatusRecord record, CancellationToken token)
    {
        await _client.BrowseVideoVisitsAsync(new ApiTelehealth.BrowseVideoVisitsRequest { OrderBy = record.Message }, cancellationToken: token);
    }
}

/// <summary>Reads acme.itsm.v1.ChangeRequestWorkflowService/ListChangeRequestWorkflows and hands what it keeps to the sink of its kind.</summary>
internal static class HandOffChangeRequestWorkflowsStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient(channel);
        var response = await source.ListChangeRequestWorkflowsAsync(new ApiItsm.ListChangeRequestWorkflowsRequest() { OrderBy = "live-hand-off-change-request-workflows-status" }, cancellationToken: token);
        var record = new HandOffChangeRequestWorkflowsStatusRecord
        {
            Status = response.Result.Status,
            CorrelationId = response.Result.CorrelationId,
            Message = Text.Normalize(response.Result.Message),
        };
        var sinks = new IHandOffChangeRequestWorkflowsStatusTarget[]
        {
            new HandOffChangeRequestWorkflowsStatusEquipmentValidationTarget(new ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient(channel)),
            new HandOffChangeRequestWorkflowsStatusTravelPolicyCalculationWriter(new ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient(channel)),
            new HandOffChangeRequestWorkflowsStatusSoftwareLicenseWorkflowRelay(new ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient(channel)),
            new HandOffChangeRequestWorkflowsStatusVideoVisitQueryTarget(new ApiTelehealth.VideoVisitQueryService.VideoVisitQueryServiceClient(channel)),
        };
        await sinks[0].DeliverAsync(record, token);
    }
}
