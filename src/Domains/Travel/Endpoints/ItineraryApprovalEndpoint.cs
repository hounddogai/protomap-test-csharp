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
using ApiAssets = Acme.Assets.V1;
using ApiCommon = Acme.Common.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Routes itineraries through approval, with escalation and delegation.
/// </summary>
public sealed class ItineraryApprovalEndpoint : ApiTravel.ItineraryApprovalService.ItineraryApprovalServiceBase
{
    private readonly ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient _assetsFixedAssetCalculationService;
    private readonly ApiDispatch.DispatchOrderService.DispatchOrderServiceClient _dispatchDispatchOrderService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient _featureflagsFeatureFlagValidationService;
    private readonly ApiIdentity.UserQueryService.UserQueryServiceClient _identityUserQueryService;
    private readonly CallInvoker _invoker;
    private readonly IItineraryApprovalEndpointSendRejectItinerary0Port _itineraryApprovalEndpointSendRejectItinerary0Port;
    private readonly ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient _itsmChangeRequestWorkflowService;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient _itsmServiceAssetApprovalService;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ILogger<ItineraryApprovalEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient _travelTravelPolicyReportingService;
    private readonly ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient _travelTravelPolicySyncService;
    private readonly ApiTravel.TripWorkflowService.TripWorkflowServiceClient _travelTripWorkflowService;

    public ItineraryApprovalEndpoint(
        ApiAssets.FixedAssetCalculationService.FixedAssetCalculationServiceClient assetsFixedAssetCalculationService,
        ApiDispatch.DispatchOrderService.DispatchOrderServiceClient dispatchDispatchOrderService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiFeatureflags.FeatureFlagValidationService.FeatureFlagValidationServiceClient featureflagsFeatureFlagValidationService,
        ApiIdentity.UserQueryService.UserQueryServiceClient identityUserQueryService,
        CallInvoker invoker,
        IItineraryApprovalEndpointSendRejectItinerary0Port itineraryApprovalEndpointSendRejectItinerary0PortAdapter,
        ApiItsm.ChangeRequestWorkflowService.ChangeRequestWorkflowServiceClient itsmChangeRequestWorkflowService,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ApiItsm.ServiceAssetApprovalService.ServiceAssetApprovalServiceClient itsmServiceAssetApprovalService,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ILogger<ItineraryApprovalEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiTravel.TravelPolicyReportingService.TravelPolicyReportingServiceClient travelTravelPolicyReportingService,
        ApiTravel.TravelPolicySyncService.TravelPolicySyncServiceClient travelTravelPolicySyncService,
        ApiTravel.TripWorkflowService.TripWorkflowServiceClient travelTripWorkflowService)
    {
        _assetsFixedAssetCalculationService = assetsFixedAssetCalculationService;
        _dispatchDispatchOrderService = dispatchDispatchOrderService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _featureflagsFeatureFlagValidationService = featureflagsFeatureFlagValidationService;
        _identityUserQueryService = identityUserQueryService;
        _invoker = invoker;
        _itineraryApprovalEndpointSendRejectItinerary0Port = itineraryApprovalEndpointSendRejectItinerary0PortAdapter;
        _itsmChangeRequestWorkflowService = itsmChangeRequestWorkflowService;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _itsmServiceAssetApprovalService = itsmServiceAssetApprovalService;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _travelItinerarySyncService = travelItinerarySyncService;
        _travelTravelPolicyReportingService = travelTravelPolicyReportingService;
        _travelTravelPolicySyncService = travelTravelPolicySyncService;
        _travelTripWorkflowService = travelTripWorkflowService;
    }
    /// <summary>
    /// Approves an itinerary.
    /// </summary>
    public override async Task<ApiTravel.ApproveItineraryResponse> ApproveItinerary(ApiTravel.ApproveItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Itinerary?.BillingAddress?.PostalCode ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeesEmployeeService.UpdateEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Bank = new ApiEmployees.EmploymentContractBankSettlement
                {
                    Bic = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (request.Itinerary?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Employee?.PrimaryAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.StreamTravelPolicyMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.StreamTravelPolicyMetricsRequest.Types.Filter
            {
                Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply0.Employee?.RelationshipManager?.Email ?? ""),
                Name = (reply0.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                Phone = (reply0.Employee?.Contact?.Phone ?? ""),
            },
        };
        using var stream2 = _travelTravelPolicyReportingService.StreamTravelPolicyMetrics(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamTravelPolicyMetrics frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiItsm.RejectServiceAssetRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.ServiceAsset = new ApiItsm.ServiceAsset
        {
            Address = new ApiCommon.PostalAddress
            {
                CountryCode = (reply1.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
            },
            Audit = new ApiCommon.AuditInfo
            {
                CreatedBy = (reply0.Employee?.Audit?.CreatedBy ?? ""),
                UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
            },
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (request.Itinerary?.Counterparty?.DisplayName ?? ""),
            },
        };
        using var rpc3 = _itsmServiceAssetApprovalService.RejectServiceAssetAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.ApproveItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
                Number = (reply0.Employee?.Passport?.Number ?? ""),
            },
            Outcome = new ApiTravel.ApproveItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ApproveItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an itinerary.
    /// </summary>
    public override async Task<ApiTravel.CommentItineraryApprovalResponse> CommentItineraryApproval(ApiTravel.CommentItineraryApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentItineraryApproval for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.ValidateFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Itinerary?.Audit?.CreatedBy ?? ""),
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Itinerary?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _featureflagsFeatureFlagValidationService.ValidateFeatureFlagAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.FeatureFlag?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.CommentItineraryApprovalResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
                Lines = { new ApiTravel.ItineraryLine
                {
                    Amount = (reply0.FeatureFlag?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
                Status = (request.Itinerary?.Status ?? (ApiTravel.ItineraryStatus)0),
            },
            Outcome = new ApiTravel.CommentItineraryApprovalResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "CommentItineraryApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.DelegateItineraryApprovalResponse> DelegateItineraryApproval(ApiTravel.DelegateItineraryApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DelegateItineraryApproval for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.PeekUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Details?.Approver?.UserId ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _identityUserQueryService.PeekUserAsync);
        var response = new ApiTravel.DelegateItineraryApprovalResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.User?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    Number = (request.Itinerary?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "DelegateItineraryApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an itinerary.
    /// </summary>
    public override async Task<ApiTravel.EscalateItineraryResponse> EscalateItinerary(ApiTravel.EscalateItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.EstimateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                },
                Bank = new ApiEmployees.EmploymentContractBankSettlement
                {
                    Iban = (request.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Bank => (request.Itinerary?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDispatch.UpdateDispatchOrderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DispatchOrder = new ApiDispatch.DispatchOrder
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Itinerary?.BillingAddress?.Region ?? ""),
                },
                Card = new ApiDispatch.DispatchOrderCardSettlement
                {
                    Token = (reply0.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (reply0.EmploymentContract?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _dispatchDispatchOrderService.UpdateDispatchOrderAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAssets.RecalculateFixedAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FixedAsset = new ApiAssets.FixedAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiAssets.RecalculateFixedAssetsRequest, ApiAssets.RecalculateFixedAssetsResponse>(
            MethodType.Unary, "acme.assets.v1" + ".FixedAssetCalculationService", "RecalculateFixedAssets",
            Wire.Marshaller<ApiAssets.RecalculateFixedAssetsRequest>(), Wire.Marshaller<ApiAssets.RecalculateFixedAssetsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.EscalateItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.DispatchOrder?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply2.FixedAsset?.Address?.Region ?? ""),
                },
                DueAt = (reply0.EmploymentContract?.DueAt ?? new Timestamp()),
                ItineraryId = (request.ItineraryId ?? ""),
                Lines = { new ApiTravel.ItineraryLine
                {
                    Quantity = (reply2.FixedAsset?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
                Total = (reply1.DispatchOrder?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiTravel.EscalateItineraryResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "EscalateItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an itinerary.
    /// </summary>
    public override async Task<ApiTravel.GetItineraryApprovalStatusResponse> GetItineraryApprovalStatus(ApiTravel.GetItineraryApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ItineraryId, "itinerary_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetItineraryApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", request.ItineraryId,
            "GetItineraryApprovalStatus", "accepted", token);
        var input0 = new ApiLedger.SearchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.SearchLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.ItineraryId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiLedger.SearchLedgerAccountsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _ledgerLedgerAccountService.SearchLedgerAccountsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetItineraryApprovalStatusResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
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
    /// Lists itineraries a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListPendingItinerariesResponse> ListPendingItineraries(ApiTravel.ListPendingItinerariesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingItineraries for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "itinerary", "batch",
            "ListPendingItineraries", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.ListPendingItinerariesRequest, ApiTravel.ListTravelPolicyReportsRequest>(value => new ApiTravel.ListTravelPolicyReportsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.ListTravelPolicyReportsRequest.Types.Filter
            {
                CreatedAfter = (value.Filter?.CreatedAfter ?? new Timestamp()),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _travelTravelPolicyReportingService.ListTravelPolicyReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ListPendingItinerariesResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                TotalSize = (reply0.PageInfo?.TotalSize ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "ListPendingItineraries", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an itinerary.
    /// </summary>
    public override async Task<ApiTravel.RecallItineraryResponse> RecallItinerary(ApiTravel.RecallItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecallItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CancelIncidentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmIncidentWorkflowService.CancelIncidentWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.SignalIncidentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Incident?.Audit?.Etag ?? ""),
                },
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Channel = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Channel => (reply0.Incident?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _itsmIncidentWorkflowService.SignalIncidentWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.RecallItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                ItineraryId = (request.ItineraryId ?? ""),
                Number = (request.Itinerary?.Number ?? ""),
            },
            Outcome = new ApiTravel.RecallItineraryResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "RecallItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an itinerary.
    /// </summary>
    public override async Task<ApiTravel.RejectItineraryResponse> RejectItinerary(ApiTravel.RejectItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.Itinerary?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Itinerary?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendRejectItinerary0Async(input0, token);
        var input1 = new ApiTravel.CancelTripWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CancelTripWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _travelTripWorkflowService.CancelTripWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiTravel.PullItinerariesResponse();
        foreach (var item in (request.Itinerary?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.ItineraryLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiTravel.PullItinerariesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiTravel.PullItinerariesRequest.Types.Details
                {
                    Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                Itinerary = new ApiTravel.Itinerary
                {
                    Lines = { new ApiTravel.ItineraryLine
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Tax = new ApiTravel.ItineraryLineTax
                        {
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                },
            };
            using var rpc2 = _travelItinerarySyncService.PullItinerariesAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var input3 = new ApiItsm.AdvanceChangeRequestWorkflowRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (request.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Trip?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiItsm.AdvanceChangeRequestWorkflowRequest>(json3);
        using var rpc3 = _itsmChangeRequestWorkflowService.AdvanceChangeRequestWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.RejectItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply1.Trip?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                    Number = (request.Itinerary?.Number ?? ""),
                },
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Outcome = new ApiTravel.RejectItineraryResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "RejectItinerary", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an itinerary.
    /// </summary>
    public override async Task<ApiTravel.SubmitItineraryResponse> SubmitItinerary(ApiTravel.SubmitItineraryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitItinerary for {Tenant}", requestContext.TenantId);
        if (request.Itinerary is { } record)
        {
            var snapshot = ItineraryMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SyncTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SyncTravelPoliciesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Itinerary?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _travelTravelPolicySyncService.SyncTravelPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.SubmitItineraryResponse
        {
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = (request.Itinerary?.Counterparty ?? new ApiTravel.TravelBookingRef()),
                ItineraryId = (request.ItineraryId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "itinerary",
            "audit", "SubmitItinerary", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.PreviewPositionResponse> SendRejectItinerary0Async(ApiEmployees.PreviewPositionRequest request, CancellationToken token)
    {
        return await _itineraryApprovalEndpointSendRejectItinerary0Port.SendAsync(request, token);
    }
}
