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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMessaging = Acme.Messaging.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTreasury = Acme.Treasury.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Routes shifts through approval, with escalation and delegation.
/// </summary>
public sealed class ShiftApprovalEndpoint : ApiScheduling.ShiftApprovalService.ShiftApprovalServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient _corporateeventsVenueApprovalService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient _helpdeskKnowledgeArticleApprovalService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient _learningCertificationApprovalService;
    private readonly ILogger<ShiftApprovalEndpoint> _logger;
    private readonly ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient _messagingConversationCalculationService;
    private readonly ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient _performanceFeedbackApprovalService;
    private readonly RetryPolicy _policy;
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _ratelimitingRateLimitCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiScheduling.ShiftQueryService.ShiftQueryServiceClient _schedulingShiftQueryService;
    private readonly Func<ApiLearning.CommentCertificationApprovalRequest, CallOptions, AsyncUnaryCall<ApiLearning.CommentCertificationApprovalResponse>> _sendSubmitShift1;
    private readonly ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient _treasuryCashPositionQueryService;
    private readonly ApiVenues.VenueValidationService.VenueValidationServiceClient _venuesVenueValidationService;
    private readonly ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient _workplacesVisitorPassReportingService;

    public ShiftApprovalEndpoint(
        NotificationBus bus,
        ApiCorporateevents.VenueApprovalService.VenueApprovalServiceClient corporateeventsVenueApprovalService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiHelpdesk.KnowledgeArticleApprovalService.KnowledgeArticleApprovalServiceClient helpdeskKnowledgeArticleApprovalService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiLearning.CertificationApprovalService.CertificationApprovalServiceClient learningCertificationApprovalService,
        ILogger<ShiftApprovalEndpoint> logger,
        ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient messagingConversationCalculationService,
        ApiPerformance.FeedbackApprovalService.FeedbackApprovalServiceClient performanceFeedbackApprovalService,
        RetryPolicy policy,
        ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient ratelimitingRateLimitCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiScheduling.ShiftQueryService.ShiftQueryServiceClient schedulingShiftQueryService,
        ApiTreasury.CashPositionQueryService.CashPositionQueryServiceClient treasuryCashPositionQueryService,
        ApiVenues.VenueValidationService.VenueValidationServiceClient venuesVenueValidationService,
        ApiWorkplaces.VisitorPassReportingService.VisitorPassReportingServiceClient workplacesVisitorPassReportingService)
    {
        _bus = bus;
        _corporateeventsVenueApprovalService = corporateeventsVenueApprovalService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _helpdeskKnowledgeArticleApprovalService = helpdeskKnowledgeArticleApprovalService;
        _identityUserQueryService = identityUserQueryService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _learningCertificationApprovalService = learningCertificationApprovalService;
        _logger = logger;
        _messagingConversationCalculationService = messagingConversationCalculationService;
        _performanceFeedbackApprovalService = performanceFeedbackApprovalService;
        _policy = policy;
        _ratelimitingRateLimitCalculationService = ratelimitingRateLimitCalculationService;
        _registry = registry;
        _repository = repository;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _schedulingShiftQueryService = schedulingShiftQueryService;
        _treasuryCashPositionQueryService = treasuryCashPositionQueryService;
        _venuesVenueValidationService = venuesVenueValidationService;
        _workplacesVisitorPassReportingService = workplacesVisitorPassReportingService;
        _sendSubmitShift1 = _learningCertificationApprovalService.CommentCertificationApprovalAsync;
    }
    /// <summary>
    /// Approves a shift.
    /// </summary>
    public override async Task<ApiScheduling.ApproveShiftResponse> ApproveShift(ApiScheduling.ApproveShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.ExplainRosterDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Shift?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Shift?.Location?.CountryCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterValidationService.ExplainRosterDecisionAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiIdentity.CountUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.CountUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Roster?.Counterparty?.EmployeeId ?? ""),
                Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _identityUserQueryService.CountUsersAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiScheduling.SuggestShiftsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiScheduling.SuggestShiftsRequest.Types.Filter
            {
                CreatedAfter = (request.Shift?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _schedulingShiftQueryService.SuggestShiftsAsync);
        var response = new ApiScheduling.ApproveShiftResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                ShiftId = (request.ShiftId ?? ""),
                Title = (request.Shift?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.ShiftId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ApproveShift", (request.Shift?.ShiftId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a shift.
    /// </summary>
    public override async Task<ApiScheduling.CommentShiftApprovalResponse> CommentShiftApproval(ApiScheduling.CommentShiftApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentShiftApproval for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(async item =>
        {
            var input0 = new ApiScheduling.FindShiftsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiScheduling.FindShiftsRequest.Types.Filter
                {
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.Shift?.Location?.Region ?? ""),
                },
            };
            using var rpc0 = _schedulingShiftQueryService.FindShiftsAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiScheduling.FindShiftsResponse();
        var response = new ApiScheduling.CommentShiftApprovalResponse
        {
            Shift = new ApiScheduling.Shift
            {
                Organizer = (request.Shift?.Organizer ?? new ApiIdentity.UserRef()),
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "CommentShiftApproval", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a shift.
    /// </summary>
    public override async Task<ApiScheduling.DelegateShiftApprovalResponse> DelegateShiftApproval(ApiScheduling.DelegateShiftApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateShiftApproval for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMessaging.RecalculateConversationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMessaging.RecalculateConversationsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Shift?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var named0 = _registry.Get<ApiMessaging.ConversationCalculationService.ConversationCalculationServiceClient>("acme.messaging.v1.ConversationCalculationService");
        using var rpc0 = named0.RecalculateConversationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.GetUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Shift?.Organizer?.UserId ?? ""),
        };
        var descriptor1 = new Method<ApiIdentity.GetUserRequest, ApiIdentity.GetUserResponse>(
            MethodType.Unary, "acme.identity.v1" + ".UserService", "GetUser",
            Wire.Marshaller<ApiIdentity.GetUserRequest>(), Wire.Marshaller<ApiIdentity.GetUserResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()).Select(async item =>
        {
            var input2 = new ApiDocuments.SubscribeDocumentTemplateUpdatesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.SubscribeDocumentTemplateUpdatesRequest.Types.Details
                {
                    Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Recipient = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (item.Contact?.Phone ?? ""),
                    },
                },
            };
            using var rpc2 = _documentsDocumentTemplateNotificationService.SubscribeDocumentTemplateUpdatesAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiDocuments.SubscribeDocumentTemplateUpdatesResponse();
        var response = new ApiScheduling.DelegateShiftApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Conversation?.Audit?.CreatedBy ?? ""),
                },
                Metadata = { (request.Shift?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.User?.RelationshipManager?.DisplayName ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "DelegateShiftApproval", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a shift.
    /// </summary>
    public override async Task<ApiScheduling.EscalateShiftResponse> EscalateShift(ApiScheduling.EscalateShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiVenues.LintVenuesResponse();
        foreach (var item in (request.Shift?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.ShiftParticipant>()))
        {
            var input0 = new ApiVenues.LintVenuesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiVenues.LintVenuesRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
            };
            using var rpc0 = _venuesVenueValidationService.LintVenuesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiWorkplaces.ShareVisitorPassReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ShareVisitorPassReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _workplacesVisitorPassReportingService.ShareVisitorPassReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.EscalateShiftToLintEmploymentContracts2(reply0, reply1);
        using var rpc2 = _employeesEmploymentContractValidationService.LintEmploymentContractsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.SimulateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SimulateEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Shift?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (reply0.Venue ?? new ApiVenues.Venue()).LocatorCase switch
                    {
                        ApiVenues.Venue.LocatorOneofCase.Address => (reply0.Venue?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    HolderName = (reply1.VisitorPass ?? new ApiWorkplaces.VisitorPass()).PaymentCase switch
                    {
                        ApiWorkplaces.VisitorPass.SettlementOneofCase.Card => (reply1.VisitorPass?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Network = (reply2.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (reply2.EmploymentContract?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _employeesEmploymentContractCalculationService.SimulateEmploymentContractAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.EscalateShiftResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.VisitorPass?.Audit?.CreatedBy ?? ""),
                    Etag = (reply3.EmploymentContract?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply2.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (reply1.VisitorPass?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply2.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Status = (request.Shift?.Status ?? (ApiScheduling.ShiftStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "EscalateShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a shift.
    /// </summary>
    public override async Task<ApiScheduling.GetShiftApprovalStatusResponse> GetShiftApprovalStatus(ApiScheduling.GetShiftApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ShiftId, "shift_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetShiftApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", request.ShiftId,
            "GetShiftApprovalStatus", "accepted", token);
        var input0 = new ApiRatelimiting.EstimateRateLimitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RateLimit = new ApiRatelimiting.RateLimit
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Scopes = { (request.Context?.Actor?.Roles ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _ratelimitingRateLimitCalculationService.EstimateRateLimitAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.RejectFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.RateLimit?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackApprovalService.RejectFeedbackAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiPerformance.RejectFeedbackRequest>("acme.scheduling.v1.ShiftApprovalService/GetShiftApprovalStatus/1", input1.Clone()), token);
        var response = new ApiScheduling.GetShiftApprovalStatusResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.RateLimit?.Audit?.CreatedBy ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.RateLimit?.Owner?.Email ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists shifts a page at a time.
    /// </summary>
    public override async Task<ApiScheduling.ListPendingShiftsResponse> ListPendingShifts(ApiScheduling.ListPendingShiftsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingShifts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "shift", "batch",
            "ListPendingShifts", "accepted", token);
        var input0 = new ApiEmployees.FindEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindEmployeesRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.FindEmployeesRequest>(json0);
        using var rpc0 = _employeesEmployeeQueryService.FindEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CommentKnowledgeArticleApprovalRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiHelpdesk.CommentKnowledgeArticleApprovalRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        input1.KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Region = (request.Filter?.Region ?? ""),
            },
            Counterparty = new ApiEmployees.EmployeeRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleApprovalService.CommentKnowledgeArticleApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.ListPendingShiftsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply0.PageInfo?.TotalSize ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Shifts = { new ApiScheduling.Shift
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "ListPendingShifts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a shift.
    /// </summary>
    public override async Task<ApiScheduling.RecallShiftResponse> RecallShift(ApiScheduling.RecallShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTreasury.SuggestCashPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTreasury.SuggestCashPositionsRequest.Types.Filter
            {
                CreatedAfter = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Shift?.Location?.Region ?? ""),
            },
        };
        using var rpc0 = _treasuryCashPositionQueryService.SuggestCashPositionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.RecallShiftResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                ShiftId = (request.ShiftId ?? ""),
                Window = (request.Shift?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "RecallShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a shift.
    /// </summary>
    public override async Task<ApiScheduling.RejectShiftResponse> RejectShift(ApiScheduling.RejectShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CompareRosterOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Shift?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Shift?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Shift?.Location?.Locality ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingRosterCalculationService.CompareRosterOptionsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiScheduling.SummarizeShiftsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            ShiftId = (request.ShiftId ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftQueryService.SummarizeShiftsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiScheduling.RejectShiftResponse
        {
            Outcome = new ApiScheduling.RejectShiftResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Roster?.BillingAddress?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply0.Roster?.Counterparty?.Email ?? ""),
                },
                ShiftId = (request.ShiftId ?? ""),
                Title = (request.Shift?.Title ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.ShiftId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "RejectShift", (request.Shift?.ShiftId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a shift.
    /// </summary>
    public override async Task<ApiScheduling.SubmitShiftResponse> SubmitShift(ApiScheduling.SubmitShiftRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitShift for {Tenant}", requestContext.TenantId);
        if (request.Shift is { } record)
        {
            var snapshot = ShiftMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.SubmitVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Shift?.Location?.PostalCode ?? ""),
                    Region = (request.Shift?.Location?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Shift?.Organizer?.DisplayName ?? ""),
                    UserId = (request.Shift?.Organizer?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _corporateeventsVenueApprovalService.SubmitVenueAsync);
        var input1 = new ApiLearning.CommentCertificationApprovalRequest
        {
            Certification = new ApiLearning.Certification
            {
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (reply0.Venue?.Owner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendSubmitShift1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.CommentFeedbackApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CommentFeedbackApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (reply0.Venue?.Owner?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _performanceFeedbackApprovalService.CommentFeedbackApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.SubmitShiftResponse
        {
            Outcome = new ApiScheduling.SubmitShiftResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Outcome?.Status ?? ""),
            },
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Location = (request.Shift?.Location ?? new ApiCommon.PostalAddress()),
                ShiftId = (request.ShiftId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Shift?.Title ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "shift",
            "audit", "SubmitShift", (request.Shift?.Title ?? ""), token);
        return response;
    }
}
