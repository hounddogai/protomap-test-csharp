using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class SoftwareLicenseQueryEndpoint
{
    /// <summary>
    /// Lists software licenses a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.BrowseSoftwareLicensesResponse> BrowseSoftwareLicenses(ApiLicensing.BrowseSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "BrowseSoftwareLicenses", "accepted", token);
        var input0 = new ApiNotifications.ExplainNotificationDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationValidationService.ExplainNotificationDecisionAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.BrowseSoftwareLicensesResponse
        {
            SoftwareLicenses = { new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Notification?.Assignee?.Email ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "BrowseSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts software licenses by status.
    /// </summary>
    public override async Task<ApiLicensing.CountSoftwareLicensesResponse> CountSoftwareLicenses(ApiLicensing.CountSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "CountSoftwareLicenses", "accepted", token);
        var response = new ApiLicensing.CountSoftwareLicensesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CountSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching software license.
    /// </summary>
    public override async Task ExportSoftwareLicenses(ApiLicensing.ExportSoftwareLicensesRequest request, IServerStreamWriter<ApiLicensing.ExportSoftwareLicensesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "ExportSoftwareLicenses", "accepted", token);
        var input0 = new ApiDocuments.CompareDocumentVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                DueAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc0 = _documentsDocumentHistoryService.CompareDocumentVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.CompareSoftwareLicenseOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Network = (reply0.Document ?? new ApiDocuments.Document()).PaymentCase switch
                    {
                        ApiDocuments.Document.SettlementOneofCase.Card => (reply0.Document?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseCalculationService.CompareSoftwareLicenseOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ExplainEntitlementDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Document?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc2 = _sendExportSoftwareLicenses2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.EstimateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EstimateEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Document?.BillingAddress?.Region ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply2.Entitlement?.Owner?.Email ?? ""),
                    UserId = (reply2.Entitlement?.Owner?.UserId ?? ""),
                },
            },
        };
        var named3 = _registry.Get<ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient>("acme.licensing.v1.EntitlementCalculationService");
        using var rpc3 = named3.EstimateEntitlementAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.ExportSoftwareLicensesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Document?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Document?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Entitlement?.Owner?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    LicenseAssignmentId = (reply1.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                },
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Limits = { new ApiLicensing.SoftwareLicenseLimit
                {
                    Name = (reply2.Entitlement?.Name ?? ""),
                } },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ExportSoftwareLicenses", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists software licenses a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.FindSoftwareLicensesResponse> FindSoftwareLicenses(ApiLicensing.FindSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "FindSoftwareLicenses", "accepted", token);
        var response = new ApiLicensing.FindSoftwareLicensesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "FindSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a software license.
    /// </summary>
    public override async Task<ApiLicensing.LookupSoftwareLicenseResponse> LookupSoftwareLicense(ApiLicensing.LookupSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SoftwareLicenseId, "software_license_id");
        _logger.LogInformation("Handling LookupSoftwareLicense for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "LookupSoftwareLicense", "accepted", token);
        var input0 = new ApiDocuments.RevertDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentRequest.Types.Details
            {
                AuthorEmail = (request.Context?.Actor?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc0 = _documentsDocumentHistoryService.RevertDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.LookupSoftwareLicenseResponse
        {
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply0.Document?.Counterparty?.DisplayName ?? ""),
                },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a software license.
    /// </summary>
    public override async Task<ApiLicensing.PeekSoftwareLicenseResponse> PeekSoftwareLicense(ApiLicensing.PeekSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SoftwareLicenseId, "software_license_id");
        _logger.LogInformation("Handling PeekSoftwareLicense for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "PeekSoftwareLicense", "accepted", token);
        var input0 = new ApiDocuments.LintSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.LintSignaturesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
                Source = (request.SoftwareLicenseId ?? ""),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.LintSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.PeekSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several software licenses at once.
    /// </summary>
    public override async Task<ApiLicensing.ResolveSoftwareLicensesResponse> ResolveSoftwareLicenses(ApiLicensing.ResolveSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "ResolveSoftwareLicenses", "accepted", token);
        var profile0 = new MappingProfile<ApiLicensing.ResolveSoftwareLicensesRequest, ApiKeymanagement.SuggestCryptoKeysRequest>(value => new ApiKeymanagement.SuggestCryptoKeysRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiKeymanagement.SuggestCryptoKeysRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                OwnerId = (value.Context?.Actor?.UserId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _keymanagementCryptoKeyQueryService.SuggestCryptoKeysAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ResolveSoftwareLicensesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists software licenses a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.SuggestSoftwareLicensesResponse> SuggestSoftwareLicenses(ApiLicensing.SuggestSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", "batch",
            "SuggestSoftwareLicenses", "accepted", token);
        var input0 = new ApiLicensing.EstimateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EstimateEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    NotifyEmail = (request.Context?.Actor?.Email ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingEntitlementCalculationService.EstimateEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.SuggestSoftwareLicensesResponse
        {
            SoftwareLicenses = { new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                },
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "SuggestSoftwareLicenses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a software license.
    /// </summary>
    public override async Task<ApiLicensing.SummarizeSoftwareLicensesResponse> SummarizeSoftwareLicenses(ApiLicensing.SummarizeSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeSoftwareLicenses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "SummarizeSoftwareLicenses", "accepted", token);
        var input0 = request.SummarizeSoftwareLicensesToCompareEntitlementOptions0();
        using var rpc0 = _licensingEntitlementCalculationService.CompareEntitlementOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.EstimateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Tax = new ApiLicensing.LicenseAssignmentLineTax
                    {
                        Amount = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply0.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentCalculationService.EstimateLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.SummarizeSoftwareLicensesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "SummarizeSoftwareLicenses", (request.SoftwareLicenseId ?? ""), token);
        return response;
    }
}
