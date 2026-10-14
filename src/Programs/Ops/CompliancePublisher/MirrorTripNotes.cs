using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiDevices = Acme.Devices.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.CompliancePublisher;

/// <summary>Where MirrorTripNotes hands each record.</summary>
internal interface IMirrorTripNotesSink
{
    Task ApplyAsync(MirrorTripNotesRecord record, CancellationToken token);
}

/// <summary>What MirrorTripNotes keeps of each QuoteTrip response.</summary>
internal sealed class MirrorTripNotesRecord
{
    public string Notes { get; init; } = "";
    public string CurrencyCode { get; init; } = "";
    public string TripId { get; init; } = "";
    public string Number { get; init; } = "";
}

/// <summary>Writes records to acme.personalization.v1.ProfileService.</summary>
internal sealed class MirrorTripNotesProfileSink(ApiPersonalization.ProfileService.ProfileServiceClient client) : IMirrorTripNotesSink
{
    public async Task ApplyAsync(MirrorTripNotesRecord record, CancellationToken token)
    {
        await client.CloneProfileAsync(new ApiPersonalization.CloneProfileRequest { Profile = new ApiPersonalization.Profile { ProfileId = record.CurrencyCode } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.devices.v1.ManagedDeviceWorkflowService.</summary>
internal sealed class MirrorTripNotesManagedDeviceWorkflowWriter(ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient client) : IMirrorTripNotesSink
{
    public async Task ApplyAsync(MirrorTripNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiDevices.PauseManagedDeviceWorkflowRequest { ManagedDeviceId = record.Notes, Reason = record.CurrencyCode, ManagedDevice = new ApiDevices.ManagedDevice { ManagedDeviceId = record.Number } }, token, client.PauseManagedDeviceWorkflowAsync);
    }
}

/// <summary>Writes records to acme.travel.v1.TravelPolicyLifecycleService.</summary>
internal sealed class MirrorTripNotesTravelPolicyLifecycleRelay(ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient client) : IMirrorTripNotesSink
{
    public async Task ApplyAsync(MirrorTripNotesRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiTravel.ReinstateTravelPolicyRequest { Reason = record.Notes, Details = new ApiTravel.ReinstateTravelPolicyRequest.Types.Details { ReasonCode = record.Number }, TravelPolicyId = record.TripId }, token, client.ReinstateTravelPolicyAsync);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class MirrorTripNotesLogOnly : IMirrorTripNotesSink
{
    public Task ApplyAsync(MirrorTripNotesRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Notes}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.travel.v1.TripCalculationService/QuoteTrip and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorTripNotes
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiTravel.TripCalculationService.TripCalculationServiceClient(channel);
        var found = await source.QuoteTripAsync(new ApiTravel.QuoteTripRequest() { Reason = "default-mirror-trip-notes" }, cancellationToken: token);
        var record = new MirrorTripNotesRecord
        {
            Notes = found.Trip.Notes,
            CurrencyCode = found.Breakdown.CurrencyCode,
            TripId = Text.Normalize(found.Trip.TripId),
            Number = found.Trip.Number,
        };
        await DeliverMirrorTripNotesAsync("managed-device-workflow", record, channel, token);
    }

    private static async Task DeliverMirrorTripNotesAsync(string kind, MirrorTripNotesRecord record, GrpcChannel channel, CancellationToken token)
    {
        IMirrorTripNotesSink sink = kind switch
        {
            "profile" => new MirrorTripNotesProfileSink(new ApiPersonalization.ProfileService.ProfileServiceClient(channel)),
            "managed-device-workflow" => new MirrorTripNotesManagedDeviceWorkflowWriter(new ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient(channel)),
            "travel-policy-lifecycle" => new MirrorTripNotesTravelPolicyLifecycleRelay(new ApiTravel.TravelPolicyLifecycleService.TravelPolicyLifecycleServiceClient(channel)),
            _ => new MirrorTripNotesLogOnly(),
        };
        await sink.ApplyAsync(record, token);
    }
}
