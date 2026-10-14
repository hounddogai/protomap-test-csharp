using Grpc.Net.Client;
using Acme.Workforce.Kit.Text;
using Acme.Workforce.Platform;
using ApiCheckout = Acme.Checkout.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiPersonalization = Acme.Personalization.V1;

namespace Acme.Workforce.Ops.AbsenceReconciler;

/// <summary>Where ExportProfilesId hands each record.</summary>
internal interface IExportProfilesIdDestination
{
    Task DeliverAsync(ExportProfilesIdRecord record, CancellationToken token);
}

/// <summary>What ExportProfilesId keeps of each FindProfiles response.</summary>
internal sealed class ExportProfilesIdRecord
{
    public string CorrelationId { get; init; } = "";
    public string NextPageToken { get; init; } = "";
    public string Status { get; init; } = "";
    public string Message { get; init; } = "";
}

/// <summary>Sends records to acme.employeeportal.v1.SelfServiceRequestNotificationService.</summary>
internal sealed class ExportProfilesIdSelfServiceRequestNotificationSink(ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient client) : IExportProfilesIdDestination
{
    public async Task DeliverAsync(ExportProfilesIdRecord record, CancellationToken token)
    {
        await Workflow.SendAsync(new ApiEmployeeportal.NotifySelfServiceRequestWatchersRequest { SelfServiceRequestId = record.Message, Reason = record.CorrelationId, Details = new ApiEmployeeportal.NotifySelfServiceRequestWatchersRequest.Types.Details { Channel = record.NextPageToken } }, token, client.NotifySelfServiceRequestWatchersAsync);
    }
}

/// <summary>Passes records to acme.checkout.v1.CheckoutSessionService.</summary>
internal sealed class ExportProfilesIdCheckoutSessionRelay(ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient client) : IExportProfilesIdDestination
{
    public async Task DeliverAsync(ExportProfilesIdRecord record, CancellationToken token)
    {
        await client.CreateCheckoutSessionAsync(new ApiCheckout.CreateCheckoutSessionRequest { CheckoutSession = new ApiCheckout.CheckoutSession { CheckoutSessionId = record.Status, Notes = record.CorrelationId }, RequestId = record.NextPageToken }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.documents.v1.DocumentTemplateAdminService.</summary>
internal sealed class ExportProfilesIdDocumentTemplateAdminSink : IExportProfilesIdDestination
{
    private readonly ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient _client;

    public ExportProfilesIdDocumentTemplateAdminSink(ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient client) => _client = client;

    public async Task DeliverAsync(ExportProfilesIdRecord record, CancellationToken token)
    {
        await _client.LockDocumentTemplateAsync(new ApiDocuments.LockDocumentTemplateRequest { Details = new ApiDocuments.LockDocumentTemplateRequest.Types.Details { ReasonCode = record.Message } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.personalization.v1.ProfileQueryService/FindProfiles and hands what it keeps to the sink of its kind.</summary>
internal static class ExportProfilesId
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiPersonalization.ProfileQueryService.ProfileQueryServiceClient(channel);
        var found = await source.FindProfilesAsync(new ApiPersonalization.FindProfilesRequest() { OrderBy = "nightly-export-profiles-id" }, cancellationToken: token);
        var record = new ExportProfilesIdRecord
        {
            CorrelationId = found.Result.CorrelationId,
            NextPageToken = found.PageInfo.NextPageToken,
            Status = Text.Normalize(found.Result.Status),
            Message = Text.Normalize(found.Result.Message),
        };
        var sinks = new List<IExportProfilesIdDestination>
        {
            new ExportProfilesIdSelfServiceRequestNotificationSink(new ApiEmployeeportal.SelfServiceRequestNotificationService.SelfServiceRequestNotificationServiceClient(channel)),
            new ExportProfilesIdCheckoutSessionRelay(new ApiCheckout.CheckoutSessionService.CheckoutSessionServiceClient(channel)),
            new ExportProfilesIdDocumentTemplateAdminSink(new ApiDocuments.DocumentTemplateAdminService.DocumentTemplateAdminServiceClient(channel)),
        };
        foreach (var sink in sinks)
            await sink.DeliverAsync(record, token);
    }
}
