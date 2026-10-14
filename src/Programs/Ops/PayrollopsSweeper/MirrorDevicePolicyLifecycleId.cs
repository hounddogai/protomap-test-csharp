using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.PayrollopsSweeper;

/// <summary>Where MirrorDevicePolicyLifecycleId hands each record.</summary>
internal interface IMirrorDevicePolicyLifecycleIdTarget
{
    Task ApplyAsync(MirrorDevicePolicyLifecycleIdRecord record, CancellationToken token);
}

/// <summary>What MirrorDevicePolicyLifecycleId keeps of each GetDevicePolicyLifecycle response.</summary>
internal sealed class MirrorDevicePolicyLifecycleIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string Status { get; init; } = "";
}

/// <summary>Hands records to acme.licensing.v1.EntitlementCalculationService.</summary>
internal sealed class MirrorDevicePolicyLifecycleIdEntitlementCalculationRelay : IMirrorDevicePolicyLifecycleIdTarget
{
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _client;

    public MirrorDevicePolicyLifecycleIdEntitlementCalculationRelay(ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorDevicePolicyLifecycleIdRecord record, CancellationToken token)
    {
        await _client.CalculateEntitlementAsync(new ApiLicensing.CalculateEntitlementRequest { Entitlement = new ApiLicensing.Entitlement { Name = record.CorrelationId }, Details = new ApiLicensing.CalculateEntitlementRequest.Types.Details { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.documents.v1.SignatureAdminService.</summary>
internal sealed class MirrorDevicePolicyLifecycleIdSignatureAdminSink : IMirrorDevicePolicyLifecycleIdTarget
{
    private readonly ApiDocuments.SignatureAdminService.SignatureAdminServiceClient _client;

    public MirrorDevicePolicyLifecycleIdSignatureAdminSink(ApiDocuments.SignatureAdminService.SignatureAdminServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorDevicePolicyLifecycleIdRecord record, CancellationToken token)
    {
        await _client.ArchiveSignatureAsync(new ApiDocuments.ArchiveSignatureRequest { SignatureId = record.Status }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.travel.v1.TravelBookingApprovalService.</summary>
internal sealed class MirrorDevicePolicyLifecycleIdTravelBookingApprovalWriter(ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient client) : IMirrorDevicePolicyLifecycleIdTarget
{
    public async Task ApplyAsync(MirrorDevicePolicyLifecycleIdRecord record, CancellationToken token)
    {
        await client.RejectTravelBookingAsync(new ApiTravel.RejectTravelBookingRequest { Details = new ApiTravel.RejectTravelBookingRequest.Types.Details { ApproverEmail = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.corporateevents.v1.RegistrationCalculationService.</summary>
internal sealed class MirrorDevicePolicyLifecycleIdRegistrationCalculationSink(ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient client) : IMirrorDevicePolicyLifecycleIdTarget
{
    public async Task ApplyAsync(MirrorDevicePolicyLifecycleIdRecord record, CancellationToken token)
    {
        await client.RecalculateRegistrationsAsync(new ApiCorporateevents.RecalculateRegistrationsRequest { RegistrationId = record.CorrelationId, Registration = new ApiCorporateevents.Registration { CurrencyCode = record.Status } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.devices.v1.DevicePolicyLifecycleService/GetDevicePolicyLifecycle and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorDevicePolicyLifecycleId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient(channel);
        var found = await source.GetDevicePolicyLifecycleAsync(new ApiDevices.GetDevicePolicyLifecycleRequest() { DevicePolicyId = "main-mirror-device-policy-lifecycle-id" }, cancellationToken: token);
        var record = new MirrorDevicePolicyLifecycleIdRecord
        {
            CorrelationId = Text.Normalize(found.Result.CorrelationId),
            Status = found.Result.Status,
        };
        await ActOnMirrorDevicePolicyLifecycleIdAsync("signature-admin", record, channel, token);
    }

    private static async Task ActOnMirrorDevicePolicyLifecycleIdAsync(string kind, MirrorDevicePolicyLifecycleIdRecord record, GrpcChannel channel, CancellationToken token)
    {
        var actions = new Dictionary<string, Func<Task>>
        {
            ["entitlement-calculation"] = () => new MirrorDevicePolicyLifecycleIdEntitlementCalculationRelay(new ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient(channel)).ApplyAsync(record, token),
            ["signature-admin"] = () => new MirrorDevicePolicyLifecycleIdSignatureAdminSink(new ApiDocuments.SignatureAdminService.SignatureAdminServiceClient(channel)).ApplyAsync(record, token),
            ["travel-booking-approval"] = () => new MirrorDevicePolicyLifecycleIdTravelBookingApprovalWriter(new ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient(channel)).ApplyAsync(record, token),
            ["registration-calculation"] = () => new MirrorDevicePolicyLifecycleIdRegistrationCalculationSink(new ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient(channel)).ApplyAsync(record, token),
        };
        await actions[kind]();
    }
}
