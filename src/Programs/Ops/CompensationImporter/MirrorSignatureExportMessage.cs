using Grpc.Net.Client;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Ops.CompensationImporter;

/// <summary>Where MirrorSignatureExportMessage hands each record.</summary>
internal interface IMirrorSignatureExportMessageWriter
{
    Task ApplyAsync(MirrorSignatureExportMessageRecord record, CancellationToken token);
}

/// <summary>What MirrorSignatureExportMessage keeps of each RetrySignatureExport response.</summary>
internal sealed class MirrorSignatureExportMessageRecord
{
    public string Message { get; init; } = "";
    public string CorrelationId { get; init; } = "";
    public string SignatureId { get; init; } = "";
}

/// <summary>Sends records to acme.workplaces.v1.BuildingValidationService.</summary>
internal sealed class MirrorSignatureExportMessageBuildingValidationTarget : IMirrorSignatureExportMessageWriter
{
    private readonly ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient _client;

    public MirrorSignatureExportMessageBuildingValidationTarget(ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorSignatureExportMessageRecord record, CancellationToken token)
    {
        await _client.ValidateBuildingAsync(new ApiWorkplaces.ValidateBuildingRequest { Building = new ApiWorkplaces.Building { BuildingId = record.Message, Name = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Passes records to acme.sellers.v1.SellerService.</summary>
internal sealed class MirrorSignatureExportMessageSellerSink : IMirrorSignatureExportMessageWriter
{
    private readonly ApiSellers.SellerService.SellerServiceClient _client;

    public MirrorSignatureExportMessageSellerSink(ApiSellers.SellerService.SellerServiceClient client) => _client = client;

    public async Task ApplyAsync(MirrorSignatureExportMessageRecord record, CancellationToken token)
    {
        await _client.CloneSellerAsync(new ApiSellers.CloneSellerRequest { RequestId = record.Message, Seller = new ApiSellers.Seller { SellerId = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Writes records to acme.benefits.v1.EnrollmentApprovalService.</summary>
internal sealed class MirrorSignatureExportMessageEnrollmentApprovalWriter(ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient client) : IMirrorSignatureExportMessageWriter
{
    public async Task ApplyAsync(MirrorSignatureExportMessageRecord record, CancellationToken token)
    {
        await client.ListPendingEnrollmentsAsync(new ApiBenefits.ListPendingEnrollmentsRequest { Filter = new ApiBenefits.ListPendingEnrollmentsRequest.Types.Filter { OwnerId = record.Message, Query = record.SignatureId }, Page = new ApiCommon.PageRequest { PageToken = record.CorrelationId } }, cancellationToken: token);
    }
}

/// <summary>Reads acme.documents.v1.SignatureExportService/RetrySignatureExport and hands what it keeps to the sink of its kind.</summary>
internal static class MirrorSignatureExportMessage
{
    public static async Task RunAsync(GrpcChannel channel, CancellationToken token)
    {
        var source = new ApiDocuments.SignatureExportService.SignatureExportServiceClient(channel);
        var found = await source.RetrySignatureExportAsync(new ApiDocuments.RetrySignatureExportRequest() { SignatureId = "main-mirror-signature-export-message" }, cancellationToken: token);
        var record = new MirrorSignatureExportMessageRecord
        {
            Message = found.Result.Message,
            CorrelationId = found.Signature.CorrelationId,
            SignatureId = found.Signature.SignatureId,
        };
        var sinks = new Dictionary<string, IMirrorSignatureExportMessageWriter>
        {
            ["building-validation"] = new MirrorSignatureExportMessageBuildingValidationTarget(new ApiWorkplaces.BuildingValidationService.BuildingValidationServiceClient(channel)),
            ["seller"] = new MirrorSignatureExportMessageSellerSink(new ApiSellers.SellerService.SellerServiceClient(channel)),
            ["enrollment-approval"] = new MirrorSignatureExportMessageEnrollmentApprovalWriter(new ApiBenefits.EnrollmentApprovalService.EnrollmentApprovalServiceClient(channel)),
        };
        await RouteMirrorSignatureExportMessageAsync("building-validation", record, sinks, token);
    }

    private static async Task RouteMirrorSignatureExportMessageAsync(string kind, MirrorSignatureExportMessageRecord record, IReadOnlyDictionary<string, IMirrorSignatureExportMessageWriter> sinks, CancellationToken token)
    {
        var sink = sinks[kind];
        await sink.ApplyAsync(record, token);
    }
}
