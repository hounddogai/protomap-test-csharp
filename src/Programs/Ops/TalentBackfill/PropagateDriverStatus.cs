using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Ops.TalentBackfill;

/// <summary>Where PropagateDriverStatus hands each record.</summary>
internal interface IPropagateDriverStatusTarget
{
    Task SendAsync(PropagateDriverStatusRecord record, CancellationToken token);
}

/// <summary>What PropagateDriverStatus keeps of each CloneDriver response.</summary>
internal sealed class PropagateDriverStatusRecord
{
    public string Status { get; init; } = "";
    public string DriverId { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class PropagateDriverStatusCheckoutSessionWriter(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) : IPropagateDriverStatusTarget
{
    public async Task SendAsync(PropagateDriverStatusRecord record, CancellationToken token)
    {
        await client.ListCheckoutSessionsAsync(new ApiCheckout.ListCheckoutSessionsRequest { Page = new ApiCommon.PageRequest { PageToken = record.Status }, Filter = new ApiCheckout.ListCheckoutSessionsRequest.Types.Filter { Region = record.Message, OwnerId = record.DriverId } }, cancellationToken: token);
    }
}

/// <summary>Hands records to acme.benefits.v1.LifeEventApprovalService.</summary>
internal sealed class PropagateDriverStatusLifeEventApprovalTarget(ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient client) : IPropagateDriverStatusTarget
{
    public async Task SendAsync(PropagateDriverStatusRecord record, CancellationToken token)
    {
        await client.SubmitLifeEventAsync(new ApiBenefits.SubmitLifeEventRequest { LifeEvent = new ApiBenefits.LifeEvent { Source = record.Message, LifeEventId = record.Status }, LifeEventId = record.DriverId }, cancellationToken: token);
    }
}

/// <summary>Sends records to acme.scheduling.v1.RosterValidationService.</summary>
internal sealed class PropagateDriverStatusRosterValidationSink(ApiScheduling.RosterValidationService.RosterValidationServiceClient client) : IPropagateDriverStatusTarget
{
    public async Task SendAsync(PropagateDriverStatusRecord record, CancellationToken token)
    {
        await client.NormalizeRosterAsync(new ApiScheduling.NormalizeRosterRequest { Roster = new ApiScheduling.Roster { RosterId = record.Status }, Details = new ApiScheduling.NormalizeRosterRequest.Types.Details { Locale = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Only logs, for kinds no service takes yet.</summary>
internal sealed class PropagateDriverStatusLogOnly : IPropagateDriverStatusTarget
{
    public Task SendAsync(PropagateDriverStatusRecord record, CancellationToken token)
    {
        Console.WriteLine($"no sink for record {record.Status}");
        return Task.CompletedTask;
    }
}

/// <summary>Reads acme.drivers.v1.DriverService/CloneDriver and hands what it keeps to the sink of its kind.</summary>
internal static class PropagateDriverStatus
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDrivers.DriverService.DriverServiceClient(channel);
        var found = await source.CloneDriverAsync(new ApiDrivers.CloneDriverRequest() { RequestId = "default-propagate-driver-status" }, cancellationToken: token);
        var record = new PropagateDriverStatusRecord
        {
            Status = found.Result.Status,
            DriverId = Text.Normalize(found.Driver.DriverId),
            Message = found.Result.Message,
        };
        await DeliverPropagateDriverStatusAsync("checkout-session", record, channel, token);
    }

    private static async Task DeliverPropagateDriverStatusAsync(string kind, PropagateDriverStatusRecord record, GrpcChannel channel, CancellationToken token)
    {
        IPropagateDriverStatusTarget sink = new PropagateDriverStatusLogOnly();
        switch (kind)
        {
            case "checkout-session":
                sink = new PropagateDriverStatusCheckoutSessionWriter(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel));
                break;
            case "life-event-approval":
                sink = new PropagateDriverStatusLifeEventApprovalTarget(new ApiBenefits.LifeEventApprovalService.LifeEventApprovalServiceClient(channel));
                break;
            case "roster-validation":
                sink = new PropagateDriverStatusRosterValidationSink(new ApiScheduling.RosterValidationService.RosterValidationServiceClient(channel));
                break;
        }
        await sink.SendAsync(record, token);
    }
}
