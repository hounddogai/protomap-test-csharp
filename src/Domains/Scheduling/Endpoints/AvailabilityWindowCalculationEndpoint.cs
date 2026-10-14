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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Computes the figures of an availability window, such as totals, taxes, and estimates.
/// </summary>
public sealed class AvailabilityWindowCalculationEndpoint : ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly IAvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port _availabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port;
    private readonly ApiCorporateevents.CorporateEventService.CorporateEventServiceClient _corporateeventsCorporateEventService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient _helpdeskSlaPolicyValidationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<AvailabilityWindowCalculationEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderService.WorkOrderServiceClient _maintenanceWorkOrderService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterCalculationService.RosterCalculationServiceClient _schedulingRosterCalculationService;
    private readonly ApiScheduling.ShiftValidationService.ShiftValidationServiceClient _schedulingShiftValidationService;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public AvailabilityWindowCalculationEndpoint(
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        IAvailabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port availabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1PortAdapter,
        ApiCorporateevents.CorporateEventService.CorporateEventServiceClient corporateeventsCorporateEventService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        ApiHelpdesk.SlaPolicyValidationService.SlaPolicyValidationServiceClient helpdeskSlaPolicyValidationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ILogger<AvailabilityWindowCalculationEndpoint> logger,
        ApiMaintenance.WorkOrderService.WorkOrderServiceClient maintenanceWorkOrderService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterCalculationService.RosterCalculationServiceClient schedulingRosterCalculationService,
        ApiScheduling.ShiftValidationService.ShiftValidationServiceClient schedulingShiftValidationService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _availabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port = availabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1PortAdapter;
        _corporateeventsCorporateEventService = corporateeventsCorporateEventService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _helpdeskSlaPolicyValidationService = helpdeskSlaPolicyValidationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _logger = logger;
        _maintenanceWorkOrderService = maintenanceWorkOrderService;
        _registry = registry;
        _repository = repository;
        _schedulingRosterCalculationService = schedulingRosterCalculationService;
        _schedulingShiftValidationService = schedulingShiftValidationService;
        _settings = settings;
        _venuesVenueQueryService = venuesVenueQueryService;
    }
    /// <summary>
    /// Calculates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.CalculateAvailabilityWindowResponse> CalculateAvailabilityWindow(ApiScheduling.CalculateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SuggestDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.AvailabilityWindow?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.AvailabilityWindowId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _documentsDocumentFolderQueryService.SuggestDocumentFoldersAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ScoreShiftRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.ScoreShiftRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.AvailabilityWindow?.Location?.PostalCode ?? ""),
                },
            },
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.AvailabilityWindow?.Organizer?.DisplayName ?? ""),
                },
                Weekly = new ApiScheduling.ShiftWeeklyRule
                {
                    Interval = (request.AvailabilityWindow?.Daily?.Interval ?? 0),
                },
            },
        };
        using var rpc1 = _schedulingShiftValidationService.ScoreShiftAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiVenues.BrowseVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.BrowseVenuesRequest.Types.Filter
            {
                Region = (reply1.Shift?.Location?.Region ?? ""),
            },
        };
        var named2 = _registry.Get<ApiVenues.VenueQueryService.VenueQueryServiceClient>("acme.venues.v1.VenueQueryService");
        using var rpc2 = named2.BrowseVenuesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiScheduling.CalculateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.Shift?.Organizer?.Email ?? ""),
                    UserId = (reply1.Shift?.Organizer?.UserId ?? ""),
                },
                Title = (request.AvailabilityWindow?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "CalculateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an availability window.
    /// </summary>
    public override async Task<ApiScheduling.CompareAvailabilityWindowOptionsResponse> CompareAvailabilityWindowOptions(ApiScheduling.CompareAvailabilityWindowOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareAvailabilityWindowOptions for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.CreateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.AvailabilityWindow?.Organizer?.DisplayName ?? ""),
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                    UserId = (request.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiAuthorization.CreateAccessPolicyRequest, ApiAuthorization.CreateAccessPolicyResponse>(
            MethodType.Unary, "acme.authorization.v1" + ".AccessPolicyService", "CreateAccessPolicy",
            Wire.Marshaller<ApiAuthorization.CreateAccessPolicyRequest>(), Wire.Marshaller<ApiAuthorization.CreateAccessPolicyResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.WatchCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.WatchCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (reply0.AccessPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
            },
            Since = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
            },
        };
        using var stream1 = _corporateeventsCorporateEventService.WatchCorporateEvents(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchCorporateEvents frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiDocuments.CheckSignatureEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CheckSignatureEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.AccessPolicy?.Owner?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.AvailabilityWindow?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureValidationService.CheckSignatureEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiMaintenance.ListWorkOrdersResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input3 = new ApiMaintenance.ListWorkOrdersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiMaintenance.ListWorkOrdersRequest.Types.Filter
                {
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc3 = _maintenanceWorkOrderService.ListWorkOrdersAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiScheduling.CompareAvailabilityWindowOptionsResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.AccessPolicy?.Audit?.CreatedBy ?? ""),
                },
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Breakdown = new ApiScheduling.CompareAvailabilityWindowOptionsResponse.Types.Breakdown
            {
                Lines = { new ApiScheduling.CompareAvailabilityWindowOptionsResponse.Types.Breakdown.Types.Line
                {
                    Amount = (reply0.AccessPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "CompareAvailabilityWindowOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.EstimateAvailabilityWindowResponse> EstimateAvailabilityWindow(ApiScheduling.EstimateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiScheduling.LintShiftsResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiScheduling.LintShiftsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Shift = new ApiScheduling.Shift
                {
                    Organizer = new ApiIdentity.UserRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                    Participants = { new ApiScheduling.ShiftParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                            Phone = (item.Contact?.Phone ?? ""),
                            PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                        },
                    } },
                },
            };
            using var rpc0 = _schedulingShiftValidationService.LintShiftsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiVenues.CountVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.CountVenuesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Shift?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.Shift?.Location?.Region ?? ""),
            },
        };
        var reply1 = await SendEstimateAvailabilityWindow1Async(input1, token);
        var reply2 = new ApiScheduling.EstimateRosterResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input2 = new ApiScheduling.EstimateRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Roster = new ApiScheduling.Roster
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.AvailabilityWindow?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        Locality = (request.AvailabilityWindow?.Location?.Locality ?? ""),
                        Recipient = (reply0.Shift?.Location?.Recipient ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _schedulingRosterCalculationService.EstimateRosterAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiHelpdesk.ScoreKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScoreKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.AvailabilityWindow?.Location?.Locality ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.AvailabilityWindow?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Shift?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiHelpdesk.ScoreKnowledgeArticleRequest>();
        using var rpc3 = _helpdeskKnowledgeArticleValidationService.ScoreKnowledgeArticleAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiScheduling.EstimateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply3.KnowledgeArticle?.BillingAddress?.Locality ?? ""),
                },
            },
            Breakdown = new ApiScheduling.EstimateAvailabilityWindowResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "EstimateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an availability window.
    /// </summary>
    public override async Task<ApiScheduling.ExplainAvailabilityWindowCalculationResponse> ExplainAvailabilityWindowCalculation(ApiScheduling.ExplainAvailabilityWindowCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainAvailabilityWindowCalculation for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CheckShiftEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                    },
                } },
                Weekly = new ApiScheduling.ShiftWeeklyRule
                {
                    Interval = (request.AvailabilityWindow ?? new ApiScheduling.AvailabilityWindow()).RecurrenceCase switch
                    {
                        ApiScheduling.AvailabilityWindow.RecurrenceOneofCase.Daily => (request.AvailabilityWindow?.Daily?.Interval ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc0 = _schedulingShiftValidationService.CheckShiftEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.ExplainAvailabilityWindowCalculationResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Shift?.Organizer?.DisplayName ?? ""),
                },
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "ExplainAvailabilityWindowCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an availability window.
    /// </summary>
    public override async Task<ApiScheduling.GetAvailabilityWindowRatesResponse> GetAvailabilityWindowRates(ApiScheduling.GetAvailabilityWindowRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.AvailabilityWindowId, "availability_window_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetAvailabilityWindowRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "availability_window", request.AvailabilityWindowId,
            "GetAvailabilityWindowRates", "accepted", token);
        var input0 = new ApiScheduling.CheckShiftEligibilityRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Shift = new ApiScheduling.Shift
        {
            Organizer = new ApiIdentity.UserRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc0 = _schedulingShiftValidationService.CheckShiftEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiScheduling.GetAvailabilityWindowRatesResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Title = (reply0.Shift?.Title ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an availability window.
    /// </summary>
    public override async Task<ApiScheduling.QuoteAvailabilityWindowResponse> QuoteAvailabilityWindow(ApiScheduling.QuoteAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.PreviewSlaPolicyResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiHelpdesk.PreviewSlaPolicyRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.PreviewSlaPolicyRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
            };
            using var rpc0 = _helpdeskSlaPolicyValidationService.PreviewSlaPolicyAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiVenues.CountVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.CountVenuesRequest.Types.Filter
            {
                CreatedAfter = (request.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _venuesVenueQueryService.CountVenuesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply2 = new ApiCorporateevents.SearchCorporateEventsResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input2 = new ApiCorporateevents.SearchCorporateEventsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiCorporateevents.SearchCorporateEventsRequest.Types.Filter
                {
                    OwnerId = (item.ParticipantId ?? ""),
                    Region = (request.AvailabilityWindow?.Location?.Region ?? ""),
                },
            };
            using var rpc2 = _corporateeventsCorporateEventService.SearchCorporateEventsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                    UserId = (request.AvailabilityWindow?.Organizer?.UserId ?? ""),
                },
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _identityUserValidationService.ScoreUserAsync);
        var response = new ApiScheduling.QuoteAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Status = (request.AvailabilityWindow?.Status ?? (ApiScheduling.AvailabilityWindowStatus)0),
            },
            Breakdown = new ApiScheduling.QuoteAvailabilityWindowResponse.Types.Breakdown
            {
                CurrencyCode = (reply3.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "QuoteAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.RecalculateAvailabilityWindowsResponse> RecalculateAvailabilityWindows(ApiScheduling.RecalculateAvailabilityWindowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateAvailabilityWindows for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()).Select(async item =>
        {
            var input0 = new ApiScheduling.ValidateShiftRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.ValidateShiftRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.Contact?.Email ?? ""),
                        Name = (item.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
                Shift = new ApiScheduling.Shift
                {
                    Participants = { new ApiScheduling.ShiftParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            PreferredChannel = (item.Contact?.PreferredChannel ?? ""),
                        },
                    } },
                },
            };
            using var rpc0 = _schedulingShiftValidationService.ValidateShiftAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiScheduling.ValidateShiftResponse();
        var reply1 = new ApiScheduling.RecalculateRostersResponse();
        foreach (var item in (request.AvailabilityWindow?.Participants ?? new Google.Protobuf.Collections.RepeatedField<ApiScheduling.AvailabilityWindowParticipant>()))
        {
            var input1 = new ApiScheduling.RecalculateRostersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.RecalculateRostersRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.AvailabilityWindow?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.Shift?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (item.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _schedulingRosterCalculationService.RecalculateRostersAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiScheduling.RecalculateAvailabilityWindowsResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Window = (request.AvailabilityWindow?.Window ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "RecalculateAvailabilityWindows", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an availability window.
    /// </summary>
    public override async Task<ApiScheduling.SimulateAvailabilityWindowResponse> SimulateAvailabilityWindow(ApiScheduling.SimulateAvailabilityWindowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateAvailabilityWindow for {Tenant}", requestContext.TenantId);
        if (request.AvailabilityWindow is { } record)
        {
            var snapshot = AvailabilityWindowMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiScheduling.CalculateRosterRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.AvailabilityWindow?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.AvailabilityWindow?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.AvailabilityWindow?.Location?.PostalCode ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiScheduling.CalculateRosterRequest, ApiScheduling.CalculateRosterResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".RosterCalculationService", "CalculateRoster",
            Wire.Marshaller<ApiScheduling.CalculateRosterRequest>(), Wire.Marshaller<ApiScheduling.CalculateRosterResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.CheckShiftEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Roster?.BillingAddress?.Recipient ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.AvailabilityWindow?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var command1 = new DispatchCommand<ApiScheduling.CheckShiftEligibilityRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _schedulingShiftValidationService.CheckShiftEligibilityAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiScheduling.SimulateAvailabilityWindowResponse
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                AvailabilityWindowId = (request.AvailabilityWindowId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Shift?.Location?.PostalCode ?? ""),
                },
                Organizer = (request.AvailabilityWindow?.Organizer ?? new ApiIdentity.UserRef()),
            },
            Breakdown = new ApiScheduling.SimulateAvailabilityWindowResponse.Types.Breakdown
            {
                Total = (reply0.Roster?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "availability_window",
            "audit", "SimulateAvailabilityWindow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiVenues.CountVenuesResponse> SendEstimateAvailabilityWindow1Async(ApiVenues.CountVenuesRequest request, CancellationToken token)
    {
        return await _availabilityWindowCalculationEndpointSendEstimateAvailabilityWindow1Port.SendAsync(request, token);
    }
}
