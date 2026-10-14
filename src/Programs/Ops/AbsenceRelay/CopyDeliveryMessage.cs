using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using ApiBenefits = Acme.Benefits.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Ops.AbsenceRelay;

/// <summary>Where CopyDeliveryMessage hands each record.</summary>
internal interface ICopyDeliveryMessageWriter
{
    Task SendAsync(CopyDeliveryMessageRecord record, CancellationToken token);
}

/// <summary>What CopyDeliveryMessage keeps of each GetDelivery response.</summary>
internal sealed class CopyDeliveryMessageRecord
{
    public string Message { get; init; } = "";
    public string Notes { get; init; } = "";
    public string CorrelationId { get; init; } = "";
}

/// <summary>Sends records to acme.travel.v1.TravelPolicyReportingService.</summary>
internal sealed class CopyDeliveryMessageTravelPolicyReportingRelay(ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient client) : ICopyDeliveryMessageWriter
{
    public async Task SendAsync(CopyDeliveryMessageRecord record, CancellationToken token)
    {
        await client.ListTravelPolicyReportsAsync(new ApiTravel.ListTravelPolicyReportsRequest { Filter = new ApiTravel.ListTravelPolicyReportsRequest.Types.Filter { OwnerId = record.Message, Region = record.CorrelationId, Query = record.Notes } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.benefits.v1.EnrollmentApprovalService.</summary>
internal sealed class CopyDeliveryMessageEnrollmentApprovalWriter(ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient client) : ICopyDeliveryMessageWriter
{
    public async Task SendAsync(CopyDeliveryMessageRecord record, CancellationToken token)
    {
        await client.EscalateEnrollmentAsync(new ApiBenefits.EscalateEnrollmentRequest { EnrollmentId = record.Message, Details = new ApiBenefits.EscalateEnrollmentRequest.Types.Details { Comment = record.Notes }, Enrollment = new ApiBenefits.Enrollment { CurrencyCode = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.deliveries.v1.DeliveryService/GetDelivery and hands what it keeps to the sink of its kind.</summary>
internal static class CopyDeliveryMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDeliveries.DeliveryService.DeliveryServiceClient(channel);
        var got = await source.GetDeliveryAsync(new ApiDeliveries.GetDeliveryRequest() { DeliveryId = "current-copy-delivery-message" }, cancellationToken: token);
        var record = new CopyDeliveryMessageRecord
        {
            Message = got.Result.Message,
            Notes = Text.Normalize(got.Delivery.Notes),
            CorrelationId = got.Result.CorrelationId,
        };
        var sinks = new Dictionary<string, ICopyDeliveryMessageWriter>
        {
            ["travel-policy-reporting"] = new CopyDeliveryMessageTravelPolicyReportingRelay(new ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient(channel)),
            ["enrollment-approval"] = new CopyDeliveryMessageEnrollmentApprovalWriter(new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(channel)),
        };
        foreach (var (kind, sink) in sinks)
        {
            if (kind != "enrollment-approval")
                continue;
            await sink.SendAsync(record, token);
        }
    }
}
