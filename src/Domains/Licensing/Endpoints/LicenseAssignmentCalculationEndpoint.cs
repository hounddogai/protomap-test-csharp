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
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Computes the figures of a license assignment, such as totals, taxes, and estimates.
/// </summary>
public sealed class LicenseAssignmentCalculationEndpoint : ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceBase
{
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<LicenseAssignmentCalculationEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;

    public LicenseAssignmentCalculationEndpoint(
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<LicenseAssignmentCalculationEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _identityUserValidationService = identityUserValidationService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _registry = registry;
        _repository = repository;
    }
    /// <summary>
    /// Calculates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.CalculateLicenseAssignmentResponse> CalculateLicenseAssignment(ApiLicensing.CalculateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ExplainNotificationDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiNotifications.NotificationComment
                {
                    CreatedAt = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        using var rpc0 = _notificationsNotificationValidationService.ExplainNotificationDecisionAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ScoreSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ScoreSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Bic = (request.LicenseAssignment?.Bank?.Bic ?? ""),
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseValidationService.ScoreSoftwareLicenseAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDocuments.ExportDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ExportDocumentsRequest.Types.Filter
            {
                Labels = { (reply1.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream2 = _documentsDocumentQueryService.ExportDocuments(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDocuments frame {Type}", change2.GetType().Name);
        }
        var response = new ApiLicensing.CalculateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Notification?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Number = (request.LicenseAssignment?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "CalculateLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.CompareLicenseAssignmentOptionsResponse> CompareLicenseAssignmentOptions(ApiLicensing.CompareLicenseAssignmentOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareLicenseAssignmentOptions for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.PreviewSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    Iban = (request.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (request.LicenseAssignment?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseValidationService.PreviewSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiLicensing.RecalculateSoftwareLicensesResponse();
        foreach (var item in (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()))
        {
            var input1 = new ApiLicensing.RecalculateSoftwareLicensesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SoftwareLicense = new ApiLicensing.SoftwareLicense
                {
                    Card = new ApiLicensing.SoftwareLicenseCardFunding
                    {
                        Last4 = (request.LicenseAssignment?.Card?.Last4 ?? ""),
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                    },
                    Limits = { new ApiLicensing.SoftwareLicenseLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc1 = _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLicensing.CompareLicenseAssignmentOptionsResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.LicenseAssignment?.IssuedAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "CompareLicenseAssignmentOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.EstimateLicenseAssignmentResponse> EstimateLicenseAssignment(ApiLicensing.EstimateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLicensing.CalculateEntitlementResponse();
        foreach (var item in (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiLicensing.CalculateEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Discount = new ApiLicensing.EntitlementDiscountAction
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                    },
                },
            };
            using var rpc0 = _licensingEntitlementCalculationService.CalculateEntitlementAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiLicensing.CheckSoftwareLicenseEligibilityRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.SoftwareLicense = new ApiLicensing.SoftwareLicense
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (reply0.Entitlement?.Audit?.UpdatedBy ?? ""),
            },
            Contact = new ApiCommon.ContactInfo
            {
                Email = (reply0.Entitlement?.Owner?.Email ?? ""),
            },
        };
        using var rpc1 = _licensingSoftwareLicenseValidationService.CheckSoftwareLicenseEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.EstimateLicenseAssignmentResponse
        {
            Breakdown = new ApiLicensing.EstimateLicenseAssignmentResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.SoftwareLicense?.CurrencyCode ?? ""),
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Metadata = { (request.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "EstimateLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.GetLicenseAssignmentRatesResponse> GetLicenseAssignmentRates(ApiLicensing.GetLicenseAssignmentRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetLicenseAssignmentRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "GetLicenseAssignmentRates", "accepted", token);
        var input0 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _identityUserValidationService.ScoreUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CancelDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.User?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.User?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.User?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateWorkflowService.CancelDocumentTemplateWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.GetLicenseAssignmentRatesResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.DocumentTemplate?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.User?.PrimaryAddress?.CountryCode ?? ""),
                    Region = (reply1.DocumentTemplate?.BillingAddress?.Region ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Number = (reply0.User?.NationalId?.Number ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.RecalculateLicenseAssignmentsResponse> RecalculateLicenseAssignments(ApiLicensing.RecalculateLicenseAssignmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateLicenseAssignments for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.QuoteSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Bank = new ApiLicensing.SoftwareLicenseBankFunding
                {
                    AccountHolder = (request.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (request.LicenseAssignment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseCalculationService.QuoteSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.QuoteEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.QuoteEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.LicenseAssignment?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (request.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingEntitlementCalculationService.QuoteEntitlementAsync);
        var input2 = new ApiLicensing.ScoreSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ScoreSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.LicenseAssignment?.BillingAddress?.Recipient ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Last4 = (request.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (request.LicenseAssignment?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseValidationService.ScoreSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.RecalculateLicenseAssignmentsResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply2.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Number = (request.LicenseAssignment?.Number ?? ""),
                Total = (reply1.Breakdown?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "RecalculateLicenseAssignments", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.SimulateLicenseAssignmentResponse> SimulateLicenseAssignment(ApiLicensing.SimulateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient>("acme.documents.v1.DocumentTemplateSyncService");
        using var rpc0 = named0.ListDocumentTemplateConflictsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.SimulateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = (request.LicenseAssignment?.BillingAddress ?? new ApiCommon.PostalAddress()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "SimulateLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
}
