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
using ApiItsm = Acme.Itsm.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformers = Acme.Performers.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Onboarding;

public sealed partial class OnboardingPlanLifecycleEndpoint
{
    /// <summary>
    /// Activates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ActivateOnboardingPlanResponse> ActivateOnboardingPlan(ApiOnboarding.ActivateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling ActivateOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "ActivateOnboardingPlan", "accepted", token);
        var input0 = new ApiOnboarding.CompareOnboardingTaskOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiOnboarding.OnboardingTaskCalculationService.OnboardingTaskCalculationServiceClient>("acme.onboarding.v1.OnboardingTaskCalculationService");
        using var rpc0 = named0.CompareOnboardingTaskOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiOnboarding.CompareOnboardingTaskOptionsRequest>("acme.onboarding.v1.OnboardingPlanLifecycleService/ActivateOnboardingPlan/0", input0.Clone()), token);
        var response = new ApiOnboarding.ActivateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.ActivateOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ActivateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ExpireOnboardingPlanResponse> ExpireOnboardingPlan(ApiOnboarding.ExpireOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling ExpireOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "ExpireOnboardingPlan", "accepted", token);
        var input0 = new ApiOnboarding.NormalizeOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.NormalizeOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiOnboarding.NormalizeOnboardingPlanRequest, ApiOnboarding.NormalizeOnboardingPlanResponse>(
            MethodType.Unary, "acme.onboarding.v1" + ".OnboardingPlanValidationService", "NormalizeOnboardingPlan",
            Wire.Marshaller<ApiOnboarding.NormalizeOnboardingPlanRequest>(), Wire.Marshaller<ApiOnboarding.NormalizeOnboardingPlanResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.CreateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.OnboardingPlan?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiVenues.CreateVenueRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _venuesVenueService.CreateVenueAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiOnboarding.ExplainOnboardingPlanCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.OnboardingPlan?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.OnboardingPlan?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (reply1.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                    {
                        ApiVenues.Venue.LocatorOneofCase.Address => (reply1.Venue?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.ExplainOnboardingPlanCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiOnboarding.ExpireOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Venue?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = (reply0.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                Metadata = { (reply2.OnboardingPlan?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Notes = (reply0.OnboardingPlan?.Notes ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.ExpireOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ExpireOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.GetOnboardingPlanLifecycleResponse> GetOnboardingPlanLifecycle(ApiOnboarding.GetOnboardingPlanLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling GetOnboardingPlanLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "GetOnboardingPlanLifecycle", "accepted", token);
        var input0 = new ApiItsm.SubmitIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmIncidentApprovalService.SubmitIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.GetOnboardingPlanLifecycleResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Incident?.Audit?.UpdatedBy ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ReinstateOnboardingPlanResponse> ReinstateOnboardingPlan(ApiOnboarding.ReinstateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling ReinstateOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "ReinstateOnboardingPlan", "accepted", token);
        var input0 = request.ReinstateOnboardingPlanToSummarizeOnboardingPlans0();
        using var rpc0 = _onboardingOnboardingPlanQueryService.SummarizeOnboardingPlansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiOnboarding.ReinstateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.ReinstateOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ReinstateOnboardingPlan", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.RenewOnboardingPlanResponse> RenewOnboardingPlan(ApiOnboarding.RenewOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling RenewOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "RenewOnboardingPlan", "accepted", token);
        var input0 = new ApiBenefits.EscalateEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.EscalateEnrollmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
            Enrollment = new ApiBenefits.Enrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendRenewOnboardingPlan0Async(input0, token);
        var input1 = new ApiOnboarding.SimulateEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.SimulateEquipmentRequest.Types.Details
            {
                CurrencyCode = (reply0.Enrollment?.CurrencyCode ?? ""),
            },
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Enrollment?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Enrollment?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Enrollment?.Holder?.DisplayName ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _onboardingEquipmentCalculationService.SimulateEquipmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.RenewOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Equipment?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Equipment?.Address?.CountryCode ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.RenewOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "RenewOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.ScheduleOnboardingPlanExpiryResponse> ScheduleOnboardingPlanExpiry(ApiOnboarding.ScheduleOnboardingPlanExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleOnboardingPlanExpiry for {Tenant}", requestContext.TenantId);
        if (request.OnboardingPlan is { } record)
        {
            var snapshot = OnboardingPlanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SubscribeDocumentUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentNotificationService.SubscribeDocumentUpdatesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiOnboarding.EstimateEquipmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Equipment = new ApiOnboarding.Equipment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.OnboardingPlan?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _onboardingEquipmentCalculationService.EstimateEquipmentAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiOnboarding.SimulateOnboardingPlanRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.OnboardingPlan = new ApiOnboarding.OnboardingPlan
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (reply0.Document?.Audit?.Etag ?? ""),
            },
        };
        using var rpc2 = _onboardingOnboardingPlanCalculationService.SimulateOnboardingPlanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiPerformers.BrowsePerformersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformers.BrowsePerformersRequest.Types.Filter
            {
                Region = (reply1.Equipment ?? new ApiOnboarding.Equipment()).LocatorCase switch
                {
                    ApiOnboarding.Equipment.LocatorOneofCase.Address => (reply1.Equipment?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _performersPerformerQueryService.BrowsePerformersAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiOnboarding.ScheduleOnboardingPlanExpiryResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = (reply0.Document?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiOnboarding.EquipmentRef
                {
                    DisplayName = (reply2.OnboardingPlan?.Counterparty?.DisplayName ?? ""),
                },
                Number = (request.OnboardingPlan?.Number ?? ""),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
                Total = (reply2.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "ScheduleOnboardingPlanExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.SuspendOnboardingPlanResponse> SuspendOnboardingPlan(ApiOnboarding.SuspendOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling SuspendOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "SuspendOnboardingPlan", "accepted", token);
        var input0 = new ApiOnboarding.ExplainOnboardingTaskDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OnboardingTask = new ApiOnboarding.OnboardingTask
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _onboardingOnboardingTaskValidationService.ExplainOnboardingTaskDecisionAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.ExportDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ExportDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (reply0.OnboardingTask?.Sla?.DueAt ?? new Timestamp()),
            },
        };
        using var stream1 = _documentsDocumentFolderQueryService.ExportDocumentFolders(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDocumentFolders frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiOnboarding.StreamOnboardingTaskMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply0.OnboardingTask?.Requester?.Email ?? ""),
                PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
            },
        };
        using var stream2 = _onboardingOnboardingTaskReportingService.StreamOnboardingTaskMetrics(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamOnboardingTaskMetrics frame {Type}", change2.GetType().Name);
        }
        var response = new ApiOnboarding.SuspendOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.OnboardingTask?.Audit?.UpdatedBy ?? ""),
                },
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.SuspendOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "SuspendOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an onboarding plan.
    /// </summary>
    public override async Task<ApiOnboarding.TerminateOnboardingPlanResponse> TerminateOnboardingPlan(ApiOnboarding.TerminateOnboardingPlanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OnboardingPlanId, "onboarding_plan_id");
        _logger.LogInformation("Handling TerminateOnboardingPlan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan", request.OnboardingPlanId,
            "TerminateOnboardingPlan", "accepted", token);
        var input0 = new ApiOnboarding.PreviewOnboardingPlanRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiOnboarding.PreviewOnboardingPlanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _sendTerminateOnboardingPlan0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.OnboardingPlan?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.OnboardingPlan?.OnboardingPlanId ?? ""),
                Region = (reply0.OnboardingPlan?.BillingAddress?.Region ?? ""),
            },
        };
        var named1 = _registry.Get<ApiDocuments.DocumentService.DocumentServiceClient>("acme.documents.v1.DocumentService");
        using var rpc1 = named1.ListDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiOnboarding.TerminateOnboardingPlanResponse
        {
            OnboardingPlan = new ApiOnboarding.OnboardingPlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.OnboardingPlan?.BillingAddress?.Recipient ?? ""),
                },
                Counterparty = (reply0.OnboardingPlan?.Counterparty ?? new ApiOnboarding.EquipmentRef()),
                OnboardingPlanId = (request.OnboardingPlanId ?? ""),
            },
            Outcome = new ApiOnboarding.TerminateOnboardingPlanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "onboarding_plan",
            "audit", "TerminateOnboardingPlan", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiBenefits.EscalateEnrollmentResponse> SendRenewOnboardingPlan0Async(ApiBenefits.EscalateEnrollmentRequest request, CancellationToken token)
    {
        return await _onboardingPlanLifecycleEndpointSendRenewOnboardingPlan0Port.SendAsync(request, token);
    }
}
