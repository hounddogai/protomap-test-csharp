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
using ApiFleet = Acme.Fleet.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class LicenseAssignmentLifecycleEndpoint
{
    /// <summary>
    /// Activates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ActivateLicenseAssignmentResponse> ActivateLicenseAssignment(ApiLicensing.ActivateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling ActivateLicenseAssignment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "ActivateLicenseAssignment", "accepted", token);
        var input0 = new ApiLicensing.DeleteLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var descriptor0 = new Method<ApiLicensing.DeleteLicenseAssignmentReportRequest, ApiLicensing.DeleteLicenseAssignmentReportResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".LicenseAssignmentReportingService", "DeleteLicenseAssignmentReport",
            Wire.Marshaller<ApiLicensing.DeleteLicenseAssignmentReportRequest>(), Wire.Marshaller<ApiLicensing.DeleteLicenseAssignmentReportResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.PreviewItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PreviewItineraryRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _travelItineraryValidationService.PreviewItineraryAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ReconcileDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc2 = _documentsDocumentFolderSyncService.ReconcileDocumentFoldersAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiLicensing.ActivateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
                Notes = (reply1.Itinerary?.Notes ?? ""),
            },
            Outcome = new ApiLicensing.ActivateLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ActivateLicenseAssignment", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ExpireLicenseAssignmentResponse> ExpireLicenseAssignment(ApiLicensing.ExpireLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling ExpireLicenseAssignment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "ExpireLicenseAssignment", "accepted", token);
        var input0 = new ApiLicensing.QuoteEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingEntitlementCalculationService.QuoteEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ExpireLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.ExpireLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ExpireLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.GetLicenseAssignmentLifecycleResponse> GetLicenseAssignmentLifecycle(ApiLicensing.GetLicenseAssignmentLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling GetLicenseAssignmentLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "GetLicenseAssignmentLifecycle", "accepted", token);
        var input0 = request.GetLicenseAssignmentLifecycleToCountLicenseAssignmentTotals0();
        using var rpc0 = _licensingLicenseAssignmentReportingService.CountLicenseAssignmentTotalsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.GetLicenseAssignmentLifecycleResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ReinstateLicenseAssignmentResponse> ReinstateLicenseAssignment(ApiLicensing.ReinstateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling ReinstateLicenseAssignment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "ReinstateLicenseAssignment", "accepted", token);
        var input0 = new ApiLicensing.DeleteLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await SendReinstateLicenseAssignment0Async(input0, token);
        var response = new ApiLicensing.ReinstateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.ReinstateLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ReinstateLicenseAssignment", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.RenewLicenseAssignmentResponse> RenewLicenseAssignment(ApiLicensing.RenewLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling RenewLicenseAssignment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "RenewLicenseAssignment", "accepted", token);
        var input0 = new ApiLicensing.SuggestSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SuggestSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingSoftwareLicenseQueryService.SuggestSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
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
                    Manager = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderSyncService.SyncDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.RenewLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.RenewLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "RenewLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ScheduleLicenseAssignmentExpiryResponse> ScheduleLicenseAssignmentExpiry(ApiLicensing.ScheduleLicenseAssignmentExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleLicenseAssignmentExpiry for {Tenant}", requestContext.TenantId);
        if (request.LicenseAssignment is { } record)
        {
            var snapshot = LicenseAssignmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SuggestEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SuggestEntitlementsRequest.Types.Filter
            {
                CreatedAfter = (request.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _licensingEntitlementQueryService.SuggestEntitlementsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiInsurance.FindPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiInsurance.FindPoliciesRequest.Types.Filter
            {
                Region = (request.LicenseAssignment?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _insurancePolicyQueryService.FindPoliciesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiLicensing.ScheduleLicenseAssignmentExpiryResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                IssuedAt = (request.LicenseAssignment?.IssuedAt ?? new Timestamp()),
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ScheduleLicenseAssignmentExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.SuspendLicenseAssignmentResponse> SuspendLicenseAssignment(ApiLicensing.SuspendLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling SuspendLicenseAssignment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "SuspendLicenseAssignment", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentTemplateRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
                IssuedAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.PushDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PullDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentFoldersRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply0.DocumentTemplate?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderSyncService.PullDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.CloneSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply1.DocumentFolder?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.DocumentTemplate?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.DocumentTemplate?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseService.CloneSoftwareLicenseAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiFleet.ListVehiclesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFleet.ListVehiclesRequest.Types.Filter
            {
                Labels = { (reply2.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply1.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _fleetVehicleService.ListVehiclesAsync);
        var response = new ApiLicensing.SuspendLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.SuspendLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "SuspendLicenseAssignment", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.TerminateLicenseAssignmentResponse> TerminateLicenseAssignment(ApiLicensing.TerminateLicenseAssignmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling TerminateLicenseAssignment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "TerminateLicenseAssignment", "accepted", token);
        var input0 = new ApiLicensing.ValidateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendTerminateLicenseAssignment0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CompareDocumentVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Lines = { new ApiDocuments.DocumentLine
                {
                    Tax = new ApiDocuments.DocumentLineTax
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
        using var rpc1 = _documentsDocumentHistoryService.CompareDocumentVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Document?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiLicensing.SimulateEntitlementRequest, ApiLicensing.SimulateEntitlementResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementCalculationService", "SimulateEntitlement",
            Wire.Marshaller<ApiLicensing.SimulateEntitlementRequest>(), Wire.Marshaller<ApiLicensing.SimulateEntitlementResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.TerminateLicenseAssignmentResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.TerminateLicenseAssignmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "TerminateLicenseAssignment", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.DeleteLicenseAssignmentReportResponse> SendReinstateLicenseAssignment0Async(ApiLicensing.DeleteLicenseAssignmentReportRequest request, CancellationToken token)
    {
        return await _licenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port.SendAsync(request, token);
    }
}
