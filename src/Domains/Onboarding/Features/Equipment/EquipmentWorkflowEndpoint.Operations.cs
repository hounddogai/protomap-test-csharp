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
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class EquipmentWorkflowEndpoint
{
    /// <summary>
    /// Advances an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.AdvanceEquipmentWorkflowResponse> AdvanceEquipmentWorkflow(ApiOnboarding.AdvanceEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiOnboarding.AdvanceEquipmentWorkflowRequest, ApiHelpdesk.ScheduleEscalationExpiryRequest>(value => new ApiHelpdesk.ScheduleEscalationExpiryRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScheduleEscalationExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (value.Equipment?.Owner?.Email ?? ""),
                    PreferredChannel = (value.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.Equipment?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (value.Equipment?.Audit?.UpdatedBy ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskEscalationLifecycleService.ScheduleEscalationExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.CountOnboardingTaskTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountOnboardingTaskTotalsRequest.Types.Filter
            {
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _onboardingOnboardingTaskReportingService.CountOnboardingTaskTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.AdvanceEquipmentWorkflowToGetEquipmentLifecycle2();
        using var rpc2 = _onboardingEquipmentLifecycleService.GetEquipmentLifecycleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.AdvanceEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Dimensions = new ApiOnboarding.EquipmentDimensions
                {
                    Height = (reply2.Equipment?.Dimensions?.Height ?? 0d),
                    Weight = (reply2.Equipment?.Dimensions?.Weight ?? 0d),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Escalation?.Requester?.Email ?? ""),
                },
                Specs = { (request.Equipment?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "AdvanceEquipmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.CancelEquipmentWorkflowResponse> CancelEquipmentWorkflow(ApiOnboarding.CancelEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.RejectOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.RejectOnboardingPlanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Equipment?.Owner?.UserId ?? ""),
                },
            },
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Equipment?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Lines = { new ApiOnboarding.OnboardingPlanLine
                {
                    Quantity = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        var reply0 = await SendCancelEquipmentWorkflow0Async(input0, token);
        var input1 = new ApiHelpdesk.DelegateKnowledgeArticleApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleApprovalService.DelegateKnowledgeArticleApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.SubmitOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    Iban = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (reply0.OnboardingPlan?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.KnowledgeArticle?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanApprovalService.SubmitOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiOnboarding.DelegateOnboardingTaskApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.DelegateOnboardingTaskApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Equipment?.Owner?.DisplayName ?? ""),
                },
            },
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiOnboarding.DelegateOnboardingTaskApprovalRequest>(json3);
        using var rpc3 = _onboardingOnboardingTaskApprovalService.DelegateOnboardingTaskApprovalAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.CancelEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply2.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Status = (request.Equipment?.Status ?? (ApiOnboarding.EquipmentStatus)0),
                Tags = { (reply3.OnboardingTask?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Outcome = new ApiOnboarding.CancelEquipmentWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "CancelEquipmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.InspectEquipmentWorkflowResponse> InspectEquipmentWorkflow(ApiOnboarding.InspectEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EquipmentId, "equipment_id");
        _logger.LogInformation("Handling InspectEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", request.EquipmentId,
            "InspectEquipmentWorkflow", "accepted", token);
        var input0 = new ApiLedger.CheckLedgerAccountEligibilityRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiLedger.CheckLedgerAccountEligibilityRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc0 = _ledgerLedgerAccountValidationService.CheckLedgerAccountEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.RejectOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    Iban = (reply0.LedgerAccount ?? new ApiLedger.LedgerAccount()).PaymentCase switch
                    {
                        ApiLedger.LedgerAccount.FundingOneofCase.Bank => (reply0.LedgerAccount?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanApprovalService.RejectOnboardingPlanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.InspectEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Code = (reply1.OnboardingPlan?.Counterparty?.Code ?? ""),
                EquipmentId = (request.EquipmentId ?? ""),
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
    /// Lists equipments a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListEquipmentWorkflowsResponse> ListEquipmentWorkflows(ApiOnboarding.ListEquipmentWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListEquipmentWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "equipment", "batch",
            "ListEquipmentWorkflows", "accepted", token);
        var input0 = new ApiLicensing.RejectLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RejectLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentApprovalService.RejectLicenseAssignmentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.ListEquipmentWorkflowsResponse
        {
            Equipments = { new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.LicenseAssignment?.Audit?.Etag ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ListEquipmentWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.PauseEquipmentWorkflowResponse> PauseEquipmentWorkflow(ApiOnboarding.PauseEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.DeleteEquipmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EquipmentId = (request.Equipment?.EquipmentId ?? ""),
            Etag = (request.Equipment?.Audit?.Etag ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _onboardingEquipmentReportingService.DeleteEquipmentReportAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.ListSignatureExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Equipment?.Owner?.UserId ?? ""),
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _documentsSignatureExportService.ListSignatureExportsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.ResumeCourseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Components = { { "primary", new ApiLearning.CourseComponent
                {
                    Description = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
            },
        };
        using var rpc2 = _sendPauseEquipmentWorkflow2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.PullItinerariesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PullItinerariesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply2.Course?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiTravel.ItineraryLine
                {
                    Quantity = (reply2.Course?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc3 = _travelItinerarySyncService.PullItinerariesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.PauseEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Name = (request.Equipment?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Course?.Owner?.DisplayName ?? ""),
                    UserId = (reply2.Course?.Owner?.UserId ?? ""),
                },
            },
            Outcome = new ApiOnboarding.PauseEquipmentWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Equipment?.Owner?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "PauseEquipmentWorkflow", (request.Equipment?.Owner?.Email ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.ResumeEquipmentWorkflowResponse> ResumeEquipmentWorkflow(ApiOnboarding.ResumeEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CountOnboardingTaskTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountOnboardingTaskTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Equipment?.Owner?.UserId ?? ""),
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _onboardingOnboardingTaskReportingService.CountOnboardingTaskTotalsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDatasets.CountDatasetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDatasets.CountDatasetsRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _datasetsDatasetQueryService.CountDatasetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ResumeEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Capacity = (request.Equipment?.Capacity ?? 0L),
                EquipmentId = (request.EquipmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "ResumeEquipmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.RetryEquipmentWorkflowResponse> RetryEquipmentWorkflow(ApiOnboarding.RetryEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.ExplainVenueDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
                Components = { { "primary", new ApiVenues.VenueComponent
                {
                    ComponentId = (request.Equipment?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                } } },
            },
        };
        using var rpc0 = _venuesVenueValidationService.ExplainVenueDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.CommentCourseEnrollmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _learningCourseEnrollmentApprovalService.CommentCourseEnrollmentApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCheckout.CountCheckoutSessionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCheckout.CountCheckoutSessionsRequest.Types.Filter
            {
                Labels = { (reply1.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply0.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                {
                    ApiVenues.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _checkoutCheckoutSessionQueryService.CountCheckoutSessionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCheckout.CountCheckoutSessionsRequest>("acme.onboarding.v1.EquipmentWorkflowService/RetryEquipmentWorkflow/2", input2.Clone()), token);
        var response = new ApiOnboarding.RetryEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Capacity = (request.Equipment?.Capacity ?? 0L),
                Components = { { "primary", new ApiOnboarding.EquipmentComponent
                {
                    Description = (reply0.Venue?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
                Dimensions = new ApiOnboarding.EquipmentDimensions
                {
                    Weight = (reply0.Venue?.Dimensions?.Weight ?? 0d),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.CourseEnrollment?.Holder?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiOnboarding.RetryEquipmentWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "RetryEquipmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.SignalEquipmentWorkflowResponse> SignalEquipmentWorkflow(ApiOnboarding.SignalEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ShareLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ShareLicenseAssignmentReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Equipment?.Owner?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentReportingService.ShareLicenseAssignmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SubmitLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    EntitlementId = (reply0.LicenseAssignment?.Counterparty?.EntitlementId ?? ""),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Description = (request.Equipment?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentApprovalService.SubmitLicenseAssignmentAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiOnboarding.CommentEquipmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Equipment = new ApiOnboarding.Equipment
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                    {
                        ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc3 = _onboardingEquipmentApprovalService.CommentEquipmentApprovalAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.SignalEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                Dimensions = new ApiOnboarding.EquipmentDimensions
                {
                    Unit = (reply3.Equipment?.Dimensions?.Unit ?? ""),
                },
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Outcome = new ApiOnboarding.SignalEquipmentWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "SignalEquipmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an equipment.
    /// </summary>
    public override async Task<ApiOnboarding.StartEquipmentWorkflowResponse> StartEquipmentWorkflow(ApiOnboarding.StartEquipmentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartEquipmentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Equipment is { } record)
        {
            var snapshot = EquipmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ExportOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.ExportOnboardingPlansRequest.Types.Filter
            {
                CreatedAfter = (request.Equipment?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Equipment?.EquipmentId ?? ""),
                Region = (request.Equipment?.Address?.Region ?? ""),
            },
        };
        using var stream0 = _onboardingOnboardingPlanQueryService.ExportOnboardingPlans(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOnboardingPlans frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLicensing.ListLicenseAssignmentReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListLicenseAssignmentReportsRequest.Types.Filter
            {
                Region = (request.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (request.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentReportingService.ListLicenseAssignmentReportsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.StartEquipmentWorkflowResponse
        {
            Equipment = new ApiOnboarding.Equipment
            {
                EquipmentId = (request.EquipmentId ?? ""),
                Owner = (request.Equipment?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "equipment",
            "audit", "StartEquipmentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiOnboarding.RejectOnboardingPlanResponse> SendCancelEquipmentWorkflow0Async(ApiOnboarding.RejectOnboardingPlanRequest request, CancellationToken token)
    {
        return await _equipmentWorkflowEndpointSendCancelEquipmentWorkflow0Port.SendAsync(request, token);
    }
}
