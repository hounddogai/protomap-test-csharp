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
using ApiCommon = Acme.Common.V1;
using ApiConfiguration = Acme.Configuration.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiSurveys = Acme.Surveys.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class EquipmentApprovalEndpoint
{
    /// <summary>
    /// Approves an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ApproveEquipmentResponse> ApproveEquipment(ApiOnboarding.ApproveEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ResumeOnboardingPlanWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ResumeOnboardingPlanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await SendApproveEquipment0Async(input0, token);
        var input1 = new ApiOnboarding.TerminateEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.TerminateEquipmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Equipment?.Owner?.Email ?? ""),
                },
            },
            EquipmentId = (request.Equipment?.EquipmentId ?? ""),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _onboardingEquipmentLifecycleService.TerminateEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ApproveEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Status = (request.Equipment?.Status ?? (ApiOnboarding.EquipmentStatus)0),
            },
            Outcome = new ApiOnboarding.ApproveEquipmentResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ApproveEquipment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.CommentEquipmentApprovalResponse> CommentEquipmentApproval(ApiOnboarding.CommentEquipmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentEquipmentApproval for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ListEscalationReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListEscalationReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Equipment?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _helpdeskEscalationReportingService.ListEscalationReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ListServiceAssetWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListServiceAssetWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _itsmServiceAssetWorkflowService.ListServiceAssetWorkflowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.CommentEquipmentApprovalResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiOnboarding.CommentEquipmentApprovalResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "CommentEquipmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.DelegateEquipmentApprovalResponse> DelegateEquipmentApproval(ApiOnboarding.DelegateEquipmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateEquipmentApproval for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ApproveReviewRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Review = new ApiPerformance.Review
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.Equipment?.Audit?.UpdatedBy ?? ""),
            },
        };
        using var rpc0 = _performanceReviewApprovalService.ApproveReviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.RenewEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.RenewEquipmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            EquipmentId = (request.EquipmentId ?? ""),
        };
        using var rpc1 = _onboardingEquipmentLifecycleService.RenewEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ScheduleLicenseAssignmentExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentLifecycleService.ScheduleLicenseAssignmentExpiryAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.DelegateEquipmentApprovalResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Dimensions = new ApiOnboarding.EquipmentDimensions
                {
                    Length = (reply1.Equipment?.Dimensions?.Length ?? 0d),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiOnboarding.DelegateEquipmentApprovalResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "DelegateEquipmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.EscalateEquipmentResponse> EscalateEquipment(ApiOnboarding.EscalateEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PullSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (request.Equipment?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureSyncService.PullSignaturesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.TerminateEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.TerminateEquipmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    UserId = (request.Equipment?.Owner?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Equipment?.Owner?.Email ?? ""),
                },
            },
            EquipmentId = (request.Equipment?.EquipmentId ?? ""),
        };
        var reply1 = await Workflow.SendAsync(input1, token, _onboardingEquipmentLifecycleService.TerminateEquipmentAsync);
        var response = new ApiOnboarding.EscalateEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Capacity = (request.Equipment?.Capacity ?? 0L),
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.EscalateEquipmentResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Equipment?.Name ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "EscalateEquipment", (request.Equipment?.Name ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.GetEquipmentApprovalStatusResponse> GetEquipmentApprovalStatus(ApiOnboarding.GetEquipmentApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling GetEquipmentApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "GetEquipmentApprovalStatus", "accepted", token);
        var input0 = new ApiAudit.LintAuditEventsRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.LintAuditEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _auditAuditEventValidationService.LintAuditEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiAudit.LintAuditEventsRequest>("acme.onboarding.v1.EquipmentApprovalService/GetEquipmentApprovalStatus/0", input0.Clone()), token);
        var response = new ApiOnboarding.GetEquipmentApprovalStatusResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.AuditEvent?.CorrelationId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists equipments a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListPendingEquipmentsResponse> ListPendingEquipments(ApiOnboarding.ListPendingEquipmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingEquipments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", "batch",
            "ListPendingEquipments", "accepted", token);
        var input0 = new ApiOnboarding.CommentOnboardingPlanApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.CommentOnboardingPlanApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                EffectiveAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiOnboarding.OnboardingPlanApprovalService.OnboardingPlanApprovalServiceClient>("acme.onboarding.v1.OnboardingPlanApprovalService");
        using var rpc0 = named0.CommentOnboardingPlanApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.ScheduleEquipmentExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Equipment = new ApiOnboarding.Equipment
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                    Region = (reply0.OnboardingPlan?.BillingAddress?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiOnboarding.ScheduleEquipmentExpiryRequest, ApiOnboarding.ScheduleEquipmentExpiryResponse>(
            MethodType.Unary, "acme.onboarding.v1" + ".EquipmentLifecycleService", "ScheduleEquipmentExpiry",
            Wire.Marshaller<ApiOnboarding.ScheduleEquipmentExpiryRequest>(), Wire.Marshaller<ApiOnboarding.ScheduleEquipmentExpiryResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ListPendingEquipmentsResponse
        {
            Equipments = { new ApiOnboarding.Equipment
            {
                Dimensions = new ApiOnboarding.EquipmentDimensions
                {
                    Width = (reply1.Equipment?.Dimensions?.Width ?? 0d),
                },
                EquipmentId = (reply0.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ListPendingEquipments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.RecallEquipmentResponse> RecallEquipment(ApiOnboarding.RecallEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ScheduleTravelBookingReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var command0 = new DispatchCommand<ApiTravel.ScheduleTravelBookingReportRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _travelTravelBookingReportingService.ScheduleTravelBookingReportAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPromotions.SimulatePromotionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPromotions.SimulatePromotionRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            Promotion = new ApiPromotions.Promotion
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _promotionsPromotionCalculationService.SimulatePromotionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.RecallEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Promotion?.Audit?.Etag ?? ""),
                },
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.RecallEquipmentResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Equipment?.Owner?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "RecallEquipment", (request.Equipment?.Owner?.UserId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.SubmitEquipmentResponse> SubmitEquipment(ApiOnboarding.SubmitEquipmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitEquipment for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiSurveys.FindSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSurveys.FindSurveysRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Equipment?.Owner?.UserId ?? ""),
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _surveysSurveyQueryService.FindSurveysAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SubmitEquipmentToRetryOnboardingPlanWorkflow1();
        using var rpc1 = _onboardingOnboardingPlanWorkflowService.RetryOnboardingPlanWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiConfiguration.CalculateConfigSetRequest
        {
            ConfigSet = new ApiConfiguration.ConfigSet
            {
                Children = { new ApiConfiguration.ConfigSet
                {
                    Children = { new ApiConfiguration.ConfigSet
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                            {
                                ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.CountryCode ?? ""),
                                _ => ""
                            },
                            Locality = (reply1.OnboardingPlan?.BillingAddress?.Locality ?? ""),
                            PostalCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                            {
                                ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _configurationConfigSetCalculationService.CalculateConfigSetAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.SubmitEquipmentResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Capacity = (request.Equipment?.Capacity ?? 0L),
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Outcome = new ApiOnboarding.SubmitEquipmentResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "SubmitEquipment", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiOnboarding.ResumeOnboardingPlanWorkflowResponse> SendApproveEquipment0Async(ApiOnboarding.ResumeOnboardingPlanWorkflowRequest request, CancellationToken token)
    {
        return await _equipmentApprovalEndpointSendApproveEquipment0Port.SendAsync(request, token);
    }
}
