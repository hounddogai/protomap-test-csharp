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
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class EntitlementWorkflowEndpoint
{
    /// <summary>
    /// Advances an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.AdvanceEntitlementWorkflowResponse> AdvanceEntitlementWorkflow(ApiLicensing.AdvanceEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.UpdateAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    ObservedAt = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                OccurredAt = (request.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventService.UpdateAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SubmitEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SubmitEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                    Email = (request.Entitlement?.Owner?.Email ?? ""),
                },
            },
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
            },
        };
        using var rpc1 = _licensingEntitlementApprovalService.SubmitEntitlementAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMessaging.ResolveConversationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor2 = new Method<ApiMessaging.ResolveConversationsRequest, ApiMessaging.ResolveConversationsResponse>(
            MethodType.Unary, "acme.messaging.v1" + ".ConversationQueryService", "ResolveConversations",
            Wire.Marshaller<ApiMessaging.ResolveConversationsRequest>(), Wire.Marshaller<ApiMessaging.ResolveConversationsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.AdvanceEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Name = (reply1.Entitlement?.Name ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Outcome = new ApiLicensing.AdvanceEntitlementWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "AdvanceEntitlementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.CancelEntitlementWorkflowResponse> CancelEntitlementWorkflow(ApiLicensing.CancelEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.BatchGetSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiLicensing.BatchGetSoftwareLicensesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingSoftwareLicenseService.BatchGetSoftwareLicensesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiLicensing.CancelEntitlementWorkflowRequest, ApiLicensing.CheckSoftwareLicenseEligibilityRequest>(value => new ApiLicensing.CheckSoftwareLicenseEligibilityRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CheckSoftwareLicenseEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Details?.Notify?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.Entitlement?.Audit?.CreatedBy ?? ""),
                    Etag = (value.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedBy = (value.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _licensingSoftwareLicenseValidationService.CheckSoftwareLicenseEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CancelEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (request.EntitlementId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
                Parameters = { (request.Entitlement?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLicensing.CancelEntitlementWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "CancelEntitlementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.InspectEntitlementWorkflowResponse> InspectEntitlementWorkflow(ApiLicensing.InspectEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EntitlementId, "entitlement_id");
        _logger.LogInformation("Handling InspectEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "entitlement", request.EntitlementId,
            "InspectEntitlementWorkflow", "accepted", token);
        var input0 = new ApiNotifications.CloneNotificationRequest
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
                Sla = new ApiNotifications.NotificationSla
                {
                    DueAt = (request.Context?.Deadline ?? new Timestamp()),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _notificationsNotificationService.CloneNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.InspectEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Notification?.Audit?.UpdatedBy ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Resumes an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.ResumeEntitlementWorkflowResponse> ResumeEntitlementWorkflow(ApiLicensing.ResumeEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ResumeEntitlementWorkflowToQuoteTrip0();
        using var rpc0 = _travelTripCalculationService.QuoteTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.DelegateEntitlementApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.DelegateEntitlementApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Entitlement?.Owner?.DisplayName ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Trip?.Audit?.Etag ?? ""),
                },
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    Reason = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Block => (request.Entitlement?.Block?.Reason ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementApprovalService.DelegateEntitlementApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.ResumeEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Parameters = { (request.Entitlement?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Outcome = new ApiLicensing.ResumeEntitlementWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "ResumeEntitlementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.RetryEntitlementWorkflowResponse> RetryEntitlementWorkflow(ApiLicensing.RetryEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SubmitSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingSoftwareLicenseApprovalService.SubmitSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiAuthorization.ScoreAccessPolicyResponse();
        foreach (var item in (request.Entitlement?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.EntitlementCondition>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiAuthorization.ScoreAccessPolicyRequest
            {
                AccessPolicy = new ApiAuthorization.AccessPolicy
                {
                    Conditions = { new ApiAuthorization.AccessPolicyCondition
                    {
                        Attribute = (item.Attribute ?? ""),
                        Negate = item.Negate,
                        Operator = (item.Operator ?? ""),
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _authorizationAccessPolicyValidationService.ScoreAccessPolicyAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLicensing.RetryEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Name = (request.Entitlement?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "RetryEntitlementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.SignalEntitlementWorkflowResponse> SignalEntitlementWorkflow(ApiLicensing.SignalEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.RejectLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RejectLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Tax = new ApiLicensing.LicenseAssignmentLineTax
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
        using var rpc0 = _licensingLicenseAssignmentApprovalService.RejectLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.EstimateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                    PostalCode = (reply0.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                },
                Card = new ApiLicensing.LicenseAssignmentCardSettlement
                {
                    Last4 = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (reply0.LicenseAssignment?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentCalculationService.EstimateLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SubmitEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SubmitEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Entitlement?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _licensingEntitlementApprovalService.SubmitEntitlementAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLicensing.SignalEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                EntitlementId = (reply2.Entitlement?.EntitlementId ?? ""),
                Owner = (request.Entitlement?.Owner ?? new ApiIdentity.UserRef()),
                Priority = (request.Entitlement?.Priority ?? 0),
            },
            Outcome = new ApiLicensing.SignalEntitlementWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "SignalEntitlementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an entitlement.
    /// </summary>
    public override async Task<ApiLicensing.StartEntitlementWorkflowResponse> StartEntitlementWorkflow(ApiLicensing.StartEntitlementWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartEntitlementWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Entitlement is { } record)
        {
            var snapshot = EntitlementMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ApproveEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Entitlement?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Entitlement?.Audit?.Etag ?? ""),
                },
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    NotifyEmail = (request.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Block => (request.Entitlement?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingEntitlementApprovalService.ApproveEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.StartEntitlementWorkflowResponse
        {
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                },
                EntitlementId = (request.EntitlementId ?? ""),
                Status = (request.Entitlement?.Status ?? (ApiLicensing.EntitlementStatus)0),
            },
            Outcome = new ApiLicensing.StartEntitlementWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "entitlement",
            "audit", "StartEntitlementWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
