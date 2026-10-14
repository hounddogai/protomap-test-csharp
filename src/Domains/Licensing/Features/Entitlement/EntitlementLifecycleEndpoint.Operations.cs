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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class EntitlementLifecycleEndpoint
{
    /// <summary>
    /// Activates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ActivateEntitlementResponse> ActivateEntitlement(ApiLicensing.ActivateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling ActivateEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "ActivateEntitlement", "accepted", token);
        var input0 = new ApiLicensing.UpdateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseService.UpdateSoftwareLicenseAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.ActivateEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Outcome = new ApiLicensing.ActivateEntitlementResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ActivateEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ExpireEntitlementResponse> ExpireEntitlement(ApiLicensing.ExpireEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling ExpireEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "ExpireEntitlement", "accepted", token);
        var input0 = new ApiDocuments.AnnotateDocumentFolderVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.AnnotateDocumentFolderVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Details?.NewOwner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderHistoryService.AnnotateDocumentFolderVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.CompareSoftwareLicenseOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CompareSoftwareLicenseOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.DocumentFolder?.Location?.Locality ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendExpireEntitlement1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SimulateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.DocumentFolder?.Location?.Locality ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient>("acme.licensing.v1.SoftwareLicenseCalculationService");
        using var rpc2 = named2.SimulateSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.ExpireEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Name = (reply0.DocumentFolder?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.SoftwareLicense?.Holder?.DisplayName ?? ""),
                    Email = (reply2.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
            Outcome = new ApiLicensing.ExpireEntitlementResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ExpireEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.RenewEntitlementResponse> RenewEntitlement(ApiLicensing.RenewEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling RenewEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "RenewEntitlement", "accepted", token);
        var input0 = new ApiNotifications.NormalizeNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.NormalizeNotificationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Notification = new ApiNotifications.Notification
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiNotifications.NormalizeNotificationRequest, ApiNotifications.NormalizeNotificationResponse>(
            MethodType.Unary, "acme.notifications.v1" + ".NotificationValidationService", "NormalizeNotification",
            Wire.Marshaller<ApiNotifications.NormalizeNotificationRequest>(), Wire.Marshaller<ApiNotifications.NormalizeNotificationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ScoreBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.Notification ?? new ApiNotifications.Notification()).OriginCase switch
                            {
                                ApiNotifications.Notification.OriginOneofCase.Customer => (reply0.Notification?.Customer?.DisplayName ?? ""),
                                _ => ""
                            },
                            Email = (reply0.Notification?.Assignee?.Email ?? ""),
                            UserId = (reply0.Notification?.Assignee?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _workplacesBuildingValidationService.ScoreBuildingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiLicensing.RenewEntitlementRequest, ApiLedger.LintLedgerAccountsRequest>(value => new ApiLedger.LintLedgerAccountsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.LintLedgerAccountsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (value.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Notification?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _ledgerLedgerAccountValidationService.LintLedgerAccountsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.RenewEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Building?.Manager?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiLicensing.RenewEntitlementResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "RenewEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ScheduleEntitlementExpiryResponse> ScheduleEntitlementExpiry(ApiLicensing.ScheduleEntitlementExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleEntitlementExpiry for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
            {
                ApiLicensing.Entitlement.ActionOneofCase.Block => (request.Entitlement?.Block?.Reason ?? ""),
                _ => ""
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureSyncService.PushSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ScheduleEntitlementExpiryResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Priority = (request.Entitlement?.Priority ?? 0),
            },
            Outcome = new ApiLicensing.ScheduleEntitlementExpiryResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ScheduleEntitlementExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.SuspendEntitlementResponse> SuspendEntitlement(ApiLicensing.SuspendEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling SuspendEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "SuspendEntitlement", "accepted", token);
        var input0 = new ApiLicensing.NormalizeLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentValidationService.NormalizeLicenseAssignmentAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.NormalizeEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementValidationService.NormalizeEntitlementAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiLicensing.QuoteSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.QuoteSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    HolderName = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (reply0.LicenseAssignment?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Limits = { new ApiLicensing.SoftwareLicenseLimit
                {
                    Amount = (reply1.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply1.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseCalculationService.QuoteSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiBenefits.SuggestEnrollmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.SuggestEnrollmentsRequest.Types.Filter
            {
                CreatedAfter = (reply2.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply2.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply0.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiBenefits.SuggestEnrollmentsRequest>();
        using var rpc3 = _benefitsEnrollmentQueryService.SuggestEnrollmentsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.SuspendEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Entitlement?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply2.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Outcome = new ApiLicensing.SuspendEntitlementResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "SuspendEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.TerminateEntitlementResponse> TerminateEntitlement(ApiLicensing.TerminateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling TerminateEntitlement for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "TerminateEntitlement", "accepted", token);
        var input0 = new ApiLicensing.BatchGetSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLicensing.BatchGetSoftwareLicensesRequest>(json0);
        using var rpc0 = _licensingSoftwareLicenseService.BatchGetSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ScheduleDocumentFolderExportRequest();
        input1.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiDocuments.ScheduleDocumentFolderExportRequest.Types.Details
        {
            Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        input1.Reason = (request.Reason ?? "");
        using var rpc1 = _documentsDocumentFolderExportService.ScheduleDocumentFolderExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.TerminateEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Outcome = new ApiLicensing.TerminateEntitlementResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "TerminateEntitlement", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
}
