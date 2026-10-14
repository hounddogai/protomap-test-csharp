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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class EquipmentLifecycleEndpoint
{
    /// <summary>
    /// Activates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ActivateEquipmentResponse> ActivateEquipment(ApiOnboarding.ActivateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling ActivateEquipment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "ActivateEquipment", "accepted", token);
        var input0 = new ApiDocuments.CalculateSignatureRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.Details?.NewOwner?.UserId ?? ""),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureCalculationService.CalculateSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ActivateEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.ActivateEquipmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ActivateEquipment", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ExpireEquipmentResponse> ExpireEquipment(ApiOnboarding.ExpireEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling ExpireEquipment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "ExpireEquipment", "accepted", token);
        var input0 = request.ExpireEquipmentToCheckEquipmentEligibility0();
        using var rpc0 = _onboardingEquipmentValidationService.CheckEquipmentEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SendDocumentFolderTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentFolderTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderNotificationService.SendDocumentFolderTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ExpireEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (reply0.Equipment?.Owner?.Email ?? ""),
                    UserId = (reply1.DocumentFolder?.Manager?.UserId ?? ""),
                },
            },
            Outcome = new ApiOnboarding.ExpireEquipmentResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ExpireEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.GetEquipmentLifecycleResponse> GetEquipmentLifecycle(ApiOnboarding.GetEquipmentLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling GetEquipmentLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "GetEquipmentLifecycle", "accepted", token);
        var input0 = new ApiOnboarding.ShareEquipmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ShareEquipmentReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            EquipmentId = (request.EquipmentId ?? ""),
        };
        using var rpc0 = _onboardingEquipmentReportingService.ShareEquipmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.GetEquipmentLifecycleResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Renews an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.RenewEquipmentResponse> RenewEquipment(ApiOnboarding.RenewEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling RenewEquipment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "RenewEquipment", "accepted", token);
        var input0 = new ApiAuthorization.LintAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.LintAccessPoliciesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _authorizationAccessPolicyValidationService.LintAccessPoliciesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiPipelines.UpdatePipelineRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                Participants = { new ApiPipelines.PipelineParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc1 = _pipelinesPipelineService.UpdatePipelineAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiOnboarding.RenewEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.AccessPolicy?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.AccessPolicy?.Audit?.Etag ?? ""),
                },
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.RenewEquipmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "RenewEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ScheduleEquipmentExpiryResponse> ScheduleEquipmentExpiry(ApiOnboarding.ScheduleEquipmentExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleEquipmentExpiry for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPatients.EstimatePatientRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Patient = new ApiPatients.Patient
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Equipment?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _patientsPatientCalculationService.EstimatePatientAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ShareEquipmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ShareEquipmentReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Patient?.RelationshipManager?.Email ?? ""),
                    Name = (reply0.Patient?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingEquipmentReportingService.ShareEquipmentReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.ListPendingVisitorPassesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.ListPendingVisitorPassesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply1.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (reply1.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _workplacesVisitorPassApprovalService.ListPendingVisitorPassesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.SendDocumentFolderTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentFolderTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Patient?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc3 = _documentsDocumentFolderNotificationService.SendDocumentFolderTestNoticeAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.ScheduleEquipmentExpiryResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Code = (request.Equipment?.Code ?? ""),
                Dimensions = new ApiOnboarding.EquipmentDimensions
                {
                    Unit = (reply1.Equipment?.Dimensions?.Unit ?? ""),
                },
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.ScheduleEquipmentExpiryResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ScheduleEquipmentExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.SuspendEquipmentResponse> SuspendEquipment(ApiOnboarding.SuspendEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling SuspendEquipment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "SuspendEquipment", "accepted", token);
        var input0 = new ApiOnboarding.ShareEquipmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ShareEquipmentReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingEquipmentReportingService.ShareEquipmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.ExplainAuditEventDecisionRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ExplainAuditEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _auditAuditEventValidationService.ExplainAuditEventDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.SuspendEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.SuspendEquipmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "SuspendEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.TerminateEquipmentResponse> TerminateEquipment(ApiOnboarding.TerminateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling TerminateEquipment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "TerminateEquipment", "accepted", token);
        var input0 = new ApiOnboarding.NormalizeEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.NormalizeEquipmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient>("acme.onboarding.v1.EquipmentValidationService");
        using var rpc0 = named0.NormalizeEquipmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.FindOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.FindOnboardingPlansRequest.Types.Filter
            {
                Region = (reply0.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (reply0.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanQueryService.FindOnboardingPlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.TerminateEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.TerminateEquipmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "TerminateEquipment", (request.Reason ?? ""), token);
        return response;
    }
}
