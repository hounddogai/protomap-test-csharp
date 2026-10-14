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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSellers = Acme.Sellers.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class LicenseAssignmentApprovalEndpoint
{
    /// <summary>
    /// Approves a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ApproveLicenseAssignmentResponse> ApproveLicenseAssignment(ApiLicensing.ApproveLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.EstimateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    EntitlementId = (request.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
                    Version = (request.LicenseAssignment?.Counterparty?.Version ?? 0L),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentCalculationService.EstimateLicenseAssignmentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLicensing.BrowseEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.BrowseEntitlementsRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply0.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingEntitlementQueryService.BrowseEntitlementsAsync);
        var response = new ApiLicensing.ApproveLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Notes = (reply0.LicenseAssignment?.Notes ?? ""),
            },
            Outcome = new ApiLicensing.ApproveLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ApproveLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.CommentLicenseAssignmentApprovalResponse> CommentLicenseAssignmentApproval(ApiLicensing.CommentLicenseAssignmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentLicenseAssignmentApproval for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _sendCommentLicenseAssignmentApproval0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiLicensing.GenerateLicenseAssignmentReportResponse();
        foreach (var item in (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()))
        {
            var input1 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
            {
                Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.GenerateLicenseAssignmentReportRequest.Types.Details
                {
                    Filters = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
                LicenseAssignmentId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
                Reason = (request.Reason ?? ""),
            };
            using var rpc1 = _licensingLicenseAssignmentReportingService.GenerateLicenseAssignmentReportAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLicensing.CommentLicenseAssignmentApprovalResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Metadata = { (request.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLicensing.CommentLicenseAssignmentApprovalResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.LicenseAssignment?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "CommentLicenseAssignmentApproval", (request.LicenseAssignment?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.DelegateLicenseAssignmentApprovalResponse> DelegateLicenseAssignmentApproval(ApiLicensing.DelegateLicenseAssignmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateLicenseAssignmentApproval for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.BrowseSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.BrowseSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor0 = new Method<ApiLicensing.BrowseSoftwareLicensesRequest, ApiLicensing.BrowseSoftwareLicensesResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseQueryService", "BrowseSoftwareLicenses",
            Wire.Marshaller<ApiLicensing.BrowseSoftwareLicensesRequest>(), Wire.Marshaller<ApiLicensing.BrowseSoftwareLicensesResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.NormalizeGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.NormalizeGoalRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiPerformance.ReviewRef
                {
                    DisplayName = (request.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiPerformance.NormalizeGoalRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _performanceGoalValidationService.NormalizeGoalAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.DelegateLicenseAssignmentApprovalResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Total = (request.LicenseAssignment?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiLicensing.DelegateLicenseAssignmentApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "DelegateLicenseAssignmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.EscalateLicenseAssignmentResponse> EscalateLicenseAssignment(ApiLicensing.EscalateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLicensing.EscalateLicenseAssignmentRequest, ApiLicensing.TerminateEntitlementRequest>(value => new ApiLicensing.TerminateEntitlementRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.TerminateEntitlementRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (value.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
            EntitlementId = (value.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _licensingEntitlementLifecycleService.TerminateEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiSellers.SearchSellersResponse();
        foreach (var item in (request.LicenseAssignment?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.LicenseAssignmentLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiSellers.SearchSellersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiSellers.SearchSellersRequest.Types.Filter
                {
                    OwnerId = (item.LineId ?? ""),
                    Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc1 = _sellersSellerService.SearchSellersAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = request.EscalateLicenseAssignmentToListSoftwareLicenses2();
        using var rpc2 = _licensingSoftwareLicenseService.ListSoftwareLicensesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ScheduleEntitlementExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply3 = await SendEscalateLicenseAssignment3Async(input3, token);
        var response = new ApiLicensing.EscalateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Status = (request.LicenseAssignment?.Status ?? (ApiLicensing.LicenseAssignmentStatus)0),
            },
            Outcome = new ApiLicensing.EscalateLicenseAssignmentResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply3.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "EscalateLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.GetLicenseAssignmentApprovalStatusResponse> GetLicenseAssignmentApprovalStatus(ApiLicensing.GetLicenseAssignmentApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling GetLicenseAssignmentApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "GetLicenseAssignmentApprovalStatus", "accepted", token);
        var input0 = new ApiLicensing.DeleteLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingLicenseAssignmentReportingService.DeleteLicenseAssignmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.GetLicenseAssignmentApprovalStatusResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists license assignments a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.ListPendingLicenseAssignmentsResponse> ListPendingLicenseAssignments(ApiLicensing.ListPendingLicenseAssignmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingLicenseAssignments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", "batch",
            "ListPendingLicenseAssignments", "accepted", token);
        var input0 = new ApiSearch.LintSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiSearch.LintSearchIndexesRequest>();
        using var rpc0 = _searchSearchIndexValidationService.LintSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ListPendingLicenseAssignmentsResponse
        {
            LicenseAssignments = { new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.SearchIndex?.Address?.PostalCode ?? ""),
                    Region = (reply0.SearchIndex?.Address?.Region ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ListPendingLicenseAssignments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.RecallLicenseAssignmentResponse> RecallLicenseAssignment(ApiLicensing.RecallLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (request.LicenseAssignment?.LicenseAssignmentId ?? ""),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLicensing.GenerateLicenseAssignmentReportRequest>(json0);
        using var rpc0 = _licensingLicenseAssignmentReportingService.GenerateLicenseAssignmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SuspendEntitlementRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiLicensing.SuspendEntitlementRequest.Types.Details
        {
            NewOwner = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                UserId = (request.Details?.Approver?.UserId ?? ""),
            },
            NotifyContact = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        input1.EntitlementId = (request.LicenseAssignment?.Counterparty?.EntitlementId ?? "");
        using var rpc1 = _licensingEntitlementLifecycleService.SuspendEntitlementAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ExplainSoftwareLicenseDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Network = (request.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (request.LicenseAssignment?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseValidationService.ExplainSoftwareLicenseDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.RecallLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Notes = (request.LicenseAssignment?.Notes ?? ""),
            },
            Outcome = new ApiLicensing.RecallLicenseAssignmentResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "RecallLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.RejectLicenseAssignmentResponse> RejectLicenseAssignment(ApiLicensing.RejectLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.NormalizeFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LicenseAssignment?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagValidationService.NormalizeFeatureFlagAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.NormalizeServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LicenseAssignment?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.FeatureFlag?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmServiceAssetValidationService.NormalizeServiceAssetAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiLicensing.PreviewEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PreviewEntitlementRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.FeatureFlag?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _licensingEntitlementValidationService.PreviewEntitlementAsync);
        var input3 = new ApiPerformance.ScoreGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ScoreGoalRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.LicenseAssignment?.BillingAddress?.CountryCode ?? ""),
                    Locality = (reply1.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply1.ServiceAsset?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc3 = _performanceGoalValidationService.ScoreGoalAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.RejectLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply2.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (reply3.Goal?.Customer?.DisplayName ?? ""),
                },
                IssuedAt = (request.LicenseAssignment?.IssuedAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.RejectLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "RejectLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.SubmitLicenseAssignmentResponse> SubmitLicenseAssignment(ApiLicensing.SubmitLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitLicenseAssignment for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.PreviewRoomBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LicenseAssignment?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (request.LicenseAssignment?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiWorkplaces.RoomBookingValidationService.RoomBookingValidationServiceClient>("acme.workplaces.v1.RoomBookingValidationService");
        using var rpc0 = named0.PreviewRoomBookingAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.RoomBooking?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var descriptor1 = new Method<ApiRecruiting.ExplainInterviewCalculationRequest, ApiRecruiting.ExplainInterviewCalculationResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".InterviewCalculationService", "ExplainInterviewCalculation",
            Wire.Marshaller<ApiRecruiting.ExplainInterviewCalculationRequest>(), Wire.Marshaller<ApiRecruiting.ExplainInterviewCalculationResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.GetEntitlementApprovalStatusRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EntitlementId = (request.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
        };
        var command2 = new DispatchCommand<ApiLicensing.GetEntitlementApprovalStatusRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _licensingEntitlementApprovalService.GetEntitlementApprovalStatusAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiLicensing.SubmitLicenseAssignmentRequest, ApiLicensing.CountLicenseAssignmentTotalsRequest>(value => new ApiLicensing.CountLicenseAssignmentTotalsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.CountLicenseAssignmentTotalsRequest.Types.Filter
            {
                CreatedAfter = (value.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.RoomBooking?.Location?.Region ?? ""),
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _licensingLicenseAssignmentReportingService.CountLicenseAssignmentTotalsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.CountLicenseAssignmentTotalsRequest>("acme.licensing.v1.LicenseAssignmentApprovalService/SubmitLicenseAssignment/3", input3.Clone()), token);
        var response = new ApiLicensing.SubmitLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.RoomBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Interview?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    EntitlementId = (reply2.Entitlement?.EntitlementId ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Metadata = { (request.LicenseAssignment?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLicensing.SubmitLicenseAssignmentResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "SubmitLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.ScheduleEntitlementExpiryResponse> SendEscalateLicenseAssignment3Async(ApiLicensing.ScheduleEntitlementExpiryRequest request, CancellationToken token)
    {
        return await _licenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port.SendAsync(request, token);
    }
}
