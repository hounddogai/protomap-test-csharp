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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class EntitlementApprovalEndpoint
{
    /// <summary>
    /// Approves an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ApproveEntitlementResponse> ApproveEntitlement(ApiLicensing.ApproveEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLicensing.ApproveEntitlementRequest, ApiMetrics.CreateMetricSeriesRequest>(value => new ApiMetrics.CreateMetricSeriesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.Entitlement?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (value.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Details?.Approver?.Email ?? ""),
                    UserId = (value.Entitlement?.Owner?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _metricsMetricSeriesService.CreateMetricSeriesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ScoreLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.MetricSeries ?? new ApiMetrics.MetricSeries()).LocatorCase switch
                    {
                        ApiMetrics.MetricSeries.LocatorOneofCase.Address => (reply0.MetricSeries?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentValidationService.ScoreLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.ApproveEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Description = (request.Entitlement?.Description ?? ""),
                EntitlementId = (request.EntitlementId ?? ""),
                Owner = (reply0.MetricSeries?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ApproveEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.CommentEntitlementApprovalResponse> CommentEntitlementApproval(ApiLicensing.CommentEntitlementApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentEntitlementApproval for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.PreviewEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Discount = new ApiLicensing.EntitlementDiscountAction
                {
                    Percent = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Discount => (request.Entitlement?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _licensingEntitlementValidationService.PreviewEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.CommentEntitlementApprovalResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Outcome = new ApiLicensing.CommentEntitlementApprovalResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "CommentEntitlementApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.DelegateEntitlementApprovalResponse> DelegateEntitlementApproval(ApiLicensing.DelegateEntitlementApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateEntitlementApproval for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RetryDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Tax = new ApiDocuments.DocumentTemplateLineTax
                    {
                        Amount = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (request.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _documentsDocumentTemplateWorkflowService.RetryDocumentTemplateWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.DelegateEntitlementApprovalResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Outcome = new ApiLicensing.DelegateEntitlementApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "DelegateEntitlementApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.EscalateEntitlementResponse> EscalateEntitlement(ApiLicensing.EscalateEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CalculateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Entitlement?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Entitlement?.Audit?.Etag ?? ""),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Limits = { new ApiCorporateevents.RegistrationLimit
                {
                    Amount = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Discount => (request.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _corporateeventsRegistrationCalculationService.CalculateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.EscalateEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Effective = (request.Entitlement?.Effective ?? new ApiCommon.TimeWindow()),
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Outcome = new ApiLicensing.EscalateEntitlementResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "EscalateEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.GetEntitlementApprovalStatusResponse> GetEntitlementApprovalStatus(ApiLicensing.GetEntitlementApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling GetEntitlementApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "GetEntitlementApprovalStatus", "accepted", token);
        var input0 = new ApiLicensing.ApproveSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ApproveSoftwareLicenseRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                ExternalAccountId = (request.Context?.TenantId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLicensing.ApproveSoftwareLicenseRequest>();
        using var rpc0 = _licensingSoftwareLicenseApprovalService.ApproveSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SuggestSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SuggestSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (reply0.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiLicensing.SuggestSoftwareLicensesRequest>(json1);
        using var rpc1 = _licensingSoftwareLicenseQueryService.SuggestSoftwareLicensesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.GetEntitlementApprovalStatusResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Recalls an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.RecallEntitlementResponse> RecallEntitlement(ApiLicensing.RecallEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSearch.FindSearchIndexesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiSearch.FindSearchIndexesRequest.Types.Filter
        {
            CreatedAfter = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
            Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (request.Details?.Approver?.UserId ?? ""),
        };
        using var rpc0 = _searchSearchIndexQueryService.FindSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.RecallEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "RecallEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.RejectEntitlementResponse> RejectEntitlement(ApiLicensing.RejectEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.RecallSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseApprovalService.RecallSoftwareLicenseAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLicensing.RejectEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "RejectEntitlement", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.SubmitEntitlementResponse> SubmitEntitlement(ApiLicensing.SubmitEntitlementRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitEntitlement for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingOfferValidationService.LintOffersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.LintOffersRequest>("acme.licensing.v1.EntitlementApprovalService/SubmitEntitlement/0", input0.Clone()), token);
        var input1 = new ApiCorporateevents.ResolveCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _corporateeventsCorporateEventQueryService.ResolveCorporateEventsAsync);
        var input2 = new ApiWorkplaces.ValidateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Entitlement?.Audit?.CreatedBy ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _sendSubmitEntitlement2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.CountOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountOnboardingPlansRequest.Types.Filter
            {
                Region = (reply2.Building?.Location?.Region ?? ""),
            },
        };
        var named3 = _registry.Get<ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient>("acme.onboarding.v1.OnboardingPlanQueryService");
        using var rpc3 = named3.CountOnboardingPlansAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.SubmitEntitlementResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Offer?.Audit?.Etag ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Building?.Manager?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiLicensing.SubmitEntitlementResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "SubmitEntitlement", (request.Reason ?? ""), token);
        return response;
    }
}
