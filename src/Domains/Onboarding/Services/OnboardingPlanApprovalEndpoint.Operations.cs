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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingPlanApprovalEndpoint
{
    /// <summary>
    /// Approves an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ApproveOnboardingPlanResponse> ApproveOnboardingPlan(ApiOnboarding.ApproveOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.SuspendEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SuspendEquipmentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
            EquipmentId = (request.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingEquipmentLifecycleService.SuspendEquipmentAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiOnboarding.StreamEquipmentMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        using var stream1 = _onboardingEquipmentReportingService.StreamEquipmentMetrics(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamEquipmentMetrics frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiIdentity.ResolveUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _identityUserQueryService.ResolveUsersAsync);
        var response = new ApiOnboarding.ApproveOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.ApproveOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ApproveOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.CommentOnboardingPlanApprovalResponse> CommentOnboardingPlanApproval(ApiOnboarding.CommentOnboardingPlanApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentOnboardingPlanApproval for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CountOnboardingTaskTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountOnboardingTaskTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.OnboardingPlan?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _sendCommentOnboardingPlanApproval0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.DeleteOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Etag = (request.OnboardingPlan?.Audit?.Etag ?? ""),
            OnboardingPlanId = (request.OnboardingPlan?.OnboardingPlanId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var named1 = _registry.Get<ApiOnboarding.OnboardingPlanService.OnboardingPlanServiceClient>("acme.onboarding.v1.OnboardingPlanService");
        using var rpc1 = named1.DeleteOnboardingPlanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.StartDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    Bic = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (request.OnboardingPlan?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateWorkflowService.StartDocumentTemplateWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.ResumeTravelBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OnboardingPlan?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.DocumentTemplate?.BillingAddress?.PostalCode ?? ""),
                },
                Card = new ApiTravel.TravelBookingCardSettlement
                {
                    Token = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (request.OnboardingPlan?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _travelTravelBookingWorkflowService.ResumeTravelBookingWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.CommentOnboardingPlanApprovalResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                DueAt = (reply3.TravelBooking?.DueAt ?? new Timestamp()),
                Notes = (reply2.DocumentTemplate?.Notes ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Total = (request.OnboardingPlan?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OnboardingPlan?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "CommentOnboardingPlanApproval", (request.OnboardingPlan?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.DelegateOnboardingPlanApprovalResponse> DelegateOnboardingPlanApproval(ApiOnboarding.DelegateOnboardingPlanApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateOnboardingPlanApproval for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.ScheduleOnboardingPlanExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Card = new ApiOnboarding.OnboardingPlanCardSettlement
                {
                    Network = (request.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Card => (request.OnboardingPlan?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    Code = (request.OnboardingPlan?.Counterparty?.Code ?? ""),
                },
            },
        };
        using var rpc0 = _onboardingOnboardingPlanLifecycleService.ScheduleOnboardingPlanExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.DelegateOnboardingPlanApprovalResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = (request.OnboardingPlan?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Number = (reply0.OnboardingPlan?.Number ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "DelegateOnboardingPlanApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.EscalateOnboardingPlanResponse> EscalateOnboardingPlan(ApiOnboarding.EscalateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.RenewEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.RenewEquipmentRequest.Types.Details
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
            },
            EquipmentId = (request.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _onboardingEquipmentLifecycleService.RenewEquipmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.EscalateOnboardingPlanToResolveOnboardingPlans1();
        using var rpc1 = _onboardingOnboardingPlanQueryService.ResolveOnboardingPlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.EscalateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Equipment?.Address?.CountryCode ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Status = (request.OnboardingPlan?.Status ?? (ApiOnboarding.OnboardingPlanStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "EscalateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingPlanApprovalStatusResponse> GetOnboardingPlanApprovalStatus(ApiOnboarding.GetOnboardingPlanApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling GetOnboardingPlanApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "GetOnboardingPlanApprovalStatus", "accepted", token);
        var input0 = new ApiOnboarding.BrowseOnboardingPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.BrowseOnboardingPlansRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OnboardingPlanId ?? ""),
            },
        };
        var reply0 = await SendGetOnboardingPlanApprovalStatus0Async(input0, token);
        var input1 = new ApiOnboarding.ExportOnboardingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.ExportOnboardingTasksRequest.Types.Filter
            {
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        using var stream1 = _onboardingOnboardingTaskQueryService.ExportOnboardingTasks(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOnboardingTasks frame {Type}", change1.GetType().Name);
        }
        var response = new ApiOnboarding.GetOnboardingPlanApprovalStatusResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists onboarding plans a page at a time.
    /// </summary>
    public override async Task<ApiOnboarding.ListPendingOnboardingPlansResponse> ListPendingOnboardingPlans(ApiOnboarding.ListPendingOnboardingPlansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingOnboardingPlans for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", "batch",
            "ListPendingOnboardingPlans", "accepted", token);
        var input0 = new ApiCorporateevents.ScheduleVenueExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.ScheduleVenueExpiryRequest>();
        using var rpc0 = _corporateeventsVenueLifecycleService.ScheduleVenueExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.RejectTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Venue?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelTravelBookingApprovalService.RejectTravelBookingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.ListPendingOnboardingPlansResponse
        {
            OnboardingPlans = { new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ListPendingOnboardingPlans", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.RecallOnboardingPlanResponse> RecallOnboardingPlan(ApiOnboarding.RecallOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.UpdateOnboardingPlanRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.OnboardingPlan = new ApiOnboarding.OnboardingPlan
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (request.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                PostalCode = (request.OnboardingPlan?.BillingAddress?.PostalCode ?? ""),
                Recipient = (request.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _onboardingOnboardingPlanService.UpdateOnboardingPlanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.NormalizeOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Bank = new ApiOnboarding.OnboardingPlanBankSettlement
                {
                    Iban = (reply0.OnboardingPlan ?? new ApiOnboarding.OnboardingPlan()).PaymentCase switch
                    {
                        ApiOnboarding.OnboardingPlan.SettlementOneofCase.Bank => (reply0.OnboardingPlan?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    EquipmentId = (reply0.OnboardingPlan?.Counterparty?.EquipmentId ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingOnboardingPlanValidationService.NormalizeOnboardingPlanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ExpireOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.ExpireOnboardingPlanRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            OnboardingPlanId = (request.OnboardingPlanId ?? ""),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingPlanLifecycleService.ExpireOnboardingPlanAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiOnboarding.RecallOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Counterparty = (request.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                Metadata = { (reply0.OnboardingPlan?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.RecallOnboardingPlanResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "RecallOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.RejectOnboardingPlanResponse> RejectOnboardingPlan(ApiOnboarding.RejectOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.CountOnboardingTaskTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiOnboarding.CountOnboardingTaskTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingPlan?.IssuedAt ?? new Timestamp()),
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingTaskReportingService.CountOnboardingTaskTotalsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLearning.ListCourseWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.ListCourseWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.OnboardingPlan?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _learningCourseWorkflowService.ListCourseWorkflowsAsync);
        var input2 = new ApiAudit.FindAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.FindAuditEventsRequest.Types.Filter
            {
                Region = (request.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _sendRejectOnboardingPlan2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.RejectOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                DueAt = (request.OnboardingPlan?.DueAt ?? new Timestamp()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.RejectOnboardingPlanResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "RejectOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.SubmitOnboardingPlanResponse> SubmitOnboardingPlan(ApiOnboarding.SubmitOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitOnboardingPlan for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.ValidateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    DisplayName = (request.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiNotifications.NotificationValidationService.NotificationValidationServiceClient>("acme.notifications.v1.NotificationValidationService");
        using var rpc0 = named0.ValidateNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.SubmitOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Number = (request.OnboardingPlan?.Number ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SubmitOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiOnboarding.BrowseOnboardingPlansResponse> SendGetOnboardingPlanApprovalStatus0Async(ApiOnboarding.BrowseOnboardingPlansRequest request, CancellationToken token)
    {
        return await _onboardingPlanApprovalEndpointSendGetOnboardingPlanApprovalStatus0Port.SendAsync(request, token);
    }
}
