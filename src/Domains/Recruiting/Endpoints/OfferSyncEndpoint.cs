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
using ApiConfiguration = Acme.Configuration.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSponsorships = Acme.Sponsorships.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

/// <summary>
/// Keeps offers in step with the systems of record in other regions.
/// </summary>
public sealed class OfferSyncEndpoint : ApiRecruiting.OfferSyncService.OfferSyncServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient _configurationConfigSetQueryService;
    private readonly ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient _corporateeventsRegistrationApprovalService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<OfferSyncEndpoint> _logger;
    private readonly IOfferSyncEndpointSendReconcileOffers0Port _offerSyncEndpointSendReconcileOffers0Port;
    private readonly ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient _ratelimitingRateLimitCalculationService;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly ApiRecruiting.OfferValidationService.OfferValidationServiceClient _recruitingOfferValidationService;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient _schedulingAvailabilityWindowWorkflowService;
    private readonly Func<ApiRecruiting.PreviewOfferRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.PreviewOfferResponse>> _sendSyncOffers1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient _sponsorshipsSponsorCalculationService;
    private readonly ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient _workplacesVisitorPassApprovalService;

    public OfferSyncEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        ApiConfiguration.ConfigSetQueryService.ConfigSetQueryServiceClient configurationConfigSetQueryService,
        ApiCorporateevents.RegistrationApprovalService.RegistrationApprovalServiceClient corporateeventsRegistrationApprovalService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ILogger<OfferSyncEndpoint> logger,
        IOfferSyncEndpointSendReconcileOffers0Port offerSyncEndpointSendReconcileOffers0PortAdapter,
        ApiRatelimiting.RateLimitCalculationService.RateLimitCalculationServiceClient ratelimitingRateLimitCalculationService,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        ApiRecruiting.OfferValidationService.OfferValidationServiceClient recruitingOfferValidationService,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowWorkflowService.AvailabilityWindowWorkflowServiceClient schedulingAvailabilityWindowWorkflowService,
        IOptions<MeshOptions> settings,
        ApiSponsorships.SponsorCalculationService.SponsorCalculationServiceClient sponsorshipsSponsorCalculationService,
        ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient workplacesVisitorPassApprovalService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _configurationConfigSetQueryService = configurationConfigSetQueryService;
        _corporateeventsRegistrationApprovalService = corporateeventsRegistrationApprovalService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _logger = logger;
        _offerSyncEndpointSendReconcileOffers0Port = offerSyncEndpointSendReconcileOffers0PortAdapter;
        _ratelimitingRateLimitCalculationService = ratelimitingRateLimitCalculationService;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _recruitingOfferValidationService = recruitingOfferValidationService;
        _repository = repository;
        _schedulingAvailabilityWindowWorkflowService = schedulingAvailabilityWindowWorkflowService;
        _settings = settings;
        _sponsorshipsSponsorCalculationService = sponsorshipsSponsorCalculationService;
        _workplacesVisitorPassApprovalService = workplacesVisitorPassApprovalService;
        _sendSyncOffers1 = _recruitingOfferValidationService.PreviewOfferAsync;
    }
    /// <summary>
    /// Returns an offer.
    /// </summary>
    public override async Task<ApiRecruiting.GetOfferSyncStateResponse> GetOfferSyncState(ApiRecruiting.GetOfferSyncStateRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetOfferSyncState for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "GetOfferSyncState", "accepted", token);
        var input0 = new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiEmployees.ResolveOrgUnitsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.GetOfferSyncStateResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                OfferId = (request.OfferId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists offers a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListOfferConflictsResponse> ListOfferConflicts(ApiRecruiting.ListOfferConflictsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListOfferConflicts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", "batch",
            "ListOfferConflicts", "accepted", token);
        var response = new ApiRecruiting.ListOfferConflictsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ListOfferConflicts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pulls an offer.
    /// </summary>
    public override async Task<ApiRecruiting.PullOffersResponse> PullOffers(ApiRecruiting.PullOffersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PullOffers for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiRecruiting.PullOffersRequest, ApiEmployees.ValidatePositionRequest>(value => new ApiEmployees.ValidatePositionRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidatePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Offer?.Counterparty?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeesPositionValidationService.ValidatePositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.PullOffersResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Lines = { new ApiRecruiting.OfferLine
                {
                    Description = (reply0.Position?.Description ?? ""),
                } },
                Notes = (request.Offer?.Notes ?? ""),
                OfferId = (request.OfferId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "PullOffers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pushs an offer.
    /// </summary>
    public override async Task<ApiRecruiting.PushOfferResponse> PushOffer(ApiRecruiting.PushOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PushOffer for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.EstimateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.EstimateJobOpeningRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Offer?.BillingAddress?.Recipient ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Card = new ApiRecruiting.JobOpeningCardSettlement
                {
                    Network = (request.Offer?.Card?.Network ?? ""),
                },
            },
        };
        using var rpc0 = _recruitingJobOpeningCalculationService.EstimateJobOpeningAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Offer?.BillingAddress?.CountryCode ?? ""),
                },
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    Token = (request.Offer?.Card?.Token ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiRecruiting.PushOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply1.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                Notes = (reply0.JobOpening?.Notes ?? ""),
                OfferId = (request.OfferId ?? ""),
                Status = (request.Offer?.Status ?? (ApiRecruiting.OfferStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "PushOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reconciles an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ReconcileOffersResponse> ReconcileOffers(ApiRecruiting.ReconcileOffersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ReconcileOffers for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRatelimiting.ExplainRateLimitCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRatelimiting.ExplainRateLimitCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Offer?.Counterparty?.Email ?? ""),
                },
            },
            RateLimit = new ApiRatelimiting.RateLimit
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply0 = await SendReconcileOffers0Async(input0, token);
        var input1 = new ApiAuthorization.FindAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.FindAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (reply0.RateLimit?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Outcome?.ReferenceId ?? ""),
                Region = (request.Offer?.BillingAddress?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _authorizationAccessPolicyQueryService.FindAccessPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ReconcileOffersResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                OfferId = (request.OfferId ?? ""),
                Total = (request.Offer?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiRecruiting.ReconcileOffersResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ReconcileOffers", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resolves an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ResolveOfferConflictResponse> ResolveOfferConflict(ApiRecruiting.ResolveOfferConflictRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResolveOfferConflict for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.BatchGetEmployeesRequest>();
        using var rpc0 = _employeesEmployeeService.BatchGetEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Offer?.Audit?.Etag ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.ValidatePositionRequest>(json1);
        using var rpc1 = _employeesPositionValidationService.ValidatePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiScheduling.PauseAvailabilityWindowWorkflowRequest();
        input2.AvailabilityWindow = new ApiScheduling.AvailabilityWindow
        {
            Location = new ApiCommon.PostalAddress
            {
                Region = (request.Offer?.BillingAddress?.Region ?? ""),
            },
        };
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc2 = _schedulingAvailabilityWindowWorkflowService.PauseAvailabilityWindowWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiConfiguration.ExportConfigSetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiConfiguration.ExportConfigSetsRequest.Types.Filter
            {
                Region = (request.Offer?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream3 = _configurationConfigSetQueryService.ExportConfigSets(input3, cancellationToken: token);
        await foreach (var change3 in stream3.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportConfigSets frame {Type}", change3.GetType().Name);
        }
        var response = new ApiRecruiting.ResolveOfferConflictResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                OfferId = (request.OfferId ?? ""),
                Total = (request.Offer?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiRecruiting.ResolveOfferConflictResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ResolveOfferConflict", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of offers with a peer.
    /// </summary>
    public override async Task StreamOfferChanges(IAsyncStreamReader<ApiRecruiting.StreamOfferChangesRequest> requests, IServerStreamWriter<ApiRecruiting.StreamOfferChangesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling StreamOfferChanges for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiEmployees.CheckPositionEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Position = new ApiEmployees.Position
                {
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = request.FrameCase switch
                        {
                            ApiRecruiting.StreamOfferChangesRequest.FrameOneofCase.Change => (request.Change?.Counterparty?.DisplayName ?? ""),
                            _ => ""
                        },
                        Email = request.FrameCase switch
                        {
                            ApiRecruiting.StreamOfferChangesRequest.FrameOneofCase.Change => (request.Change?.Counterparty?.Email ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiSponsorships.CompareSponsorOptionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiSponsorships.CompareSponsorOptionsRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        CountryCode = request.FrameCase switch
                        {
                            ApiRecruiting.StreamOfferChangesRequest.FrameOneofCase.Change => (request.Change?.BillingAddress?.CountryCode ?? ""),
                            _ => ""
                        },
                    },
                },
                Sponsor = new ApiSponsorships.Sponsor
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                        UpdatedBy = request.FrameCase switch
                        {
                            ApiRecruiting.StreamOfferChangesRequest.FrameOneofCase.Change => (request.Change?.Audit?.UpdatedBy ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc1 = _sponsorshipsSponsorCalculationService.CompareSponsorOptionsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var response = new ApiRecruiting.StreamOfferChangesResponse
            {
                Offer = new ApiRecruiting.Offer
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        PostalCode = (reply1.Sponsor?.PrimaryAddress?.PostalCode ?? ""),
                    },
                    Counterparty = new ApiEmployees.EmployeeRef
                    {
                        Email = (reply0.Position?.Owner?.Email ?? ""),
                    },
                    Metadata = { (request.Change?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Status = (request.Change?.Status ?? (ApiRecruiting.OfferStatus)0),
                },
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "offer",
                "audit", "StreamOfferChanges", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Syncs an offer.
    /// </summary>
    public override async Task<ApiRecruiting.SyncOffersResponse> SyncOffers(ApiRecruiting.SyncOffersRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SyncOffers for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Filters = { (request.Offer?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesOrgUnitQueryService.SummarizeOrgUnitsAsync);
        var input1 = new ApiRecruiting.PreviewOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Offer?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Offer?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _sendSyncOffers1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.ApproveRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Offer?.Audit?.Etag ?? ""),
                },
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    HolderName = (reply1.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Card => (reply1.Offer?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _corporateeventsRegistrationApprovalService.ApproveRegistrationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiWorkplaces.RecallVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.RecallVisitorPassRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Offer?.Counterparty?.DisplayName ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Offer?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (reply1.Offer?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiWorkplaces.RecallVisitorPassRequest, ApiWorkplaces.RecallVisitorPassResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".VisitorPassApprovalService", "RecallVisitorPass",
            Wire.Marshaller<ApiWorkplaces.RecallVisitorPassRequest>(), Wire.Marshaller<ApiWorkplaces.RecallVisitorPassResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.SyncOffersResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                OfferId = (request.OfferId ?? ""),
                Status = (request.Offer?.Status ?? (ApiRecruiting.OfferStatus)0),
            },
            Outcome = new ApiRecruiting.SyncOffersResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Offer?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "SyncOffers", (request.Offer?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiRatelimiting.ExplainRateLimitCalculationResponse> SendReconcileOffers0Async(ApiRatelimiting.ExplainRateLimitCalculationRequest request, CancellationToken token)
    {
        return await _offerSyncEndpointSendReconcileOffers0Port.SendAsync(request, token);
    }
}
