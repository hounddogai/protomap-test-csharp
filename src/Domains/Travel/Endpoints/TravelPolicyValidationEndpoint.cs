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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiSearch = Acme.Search.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Checks travel policies against the rules before anything acts on them.
/// </summary>
public sealed class TravelPolicyValidationEndpoint : ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceBase
{
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient _documentsSignatureHistoryService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ILogger<TravelPolicyValidationEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient _maintenanceWorkOrderQueryService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient _searchSearchIndexQueryService;
    private readonly Func<ApiTravel.ExplainTripCalculationRequest, CallOptions, AsyncUnaryCall<ApiTravel.ExplainTripCalculationResponse>> _sendGetTravelPolicyRules0;
    private readonly ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient _travelItineraryCalculationService;
    private readonly ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient _travelTravelBookingCalculationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TravelPolicyValidationEndpoint(
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDocuments.SignatureHistoryService.SignatureHistoryServiceClient documentsSignatureHistoryService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ILogger<TravelPolicyValidationEndpoint> logger,
        ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient maintenanceWorkOrderQueryService,
        IRecordRepository repository,
        ApiSearch.SearchIndexQueryService.SearchIndexQueryServiceClient searchSearchIndexQueryService,
        ApiTravel.ItineraryCalculationService.ItineraryCalculationServiceClient travelItineraryCalculationService,
        ApiTravel.TravelBookingCalculationService.TravelBookingCalculationServiceClient travelTravelBookingCalculationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _documentsSignatureHistoryService = documentsSignatureHistoryService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _logger = logger;
        _maintenanceWorkOrderQueryService = maintenanceWorkOrderQueryService;
        _repository = repository;
        _searchSearchIndexQueryService = searchSearchIndexQueryService;
        _travelItineraryCalculationService = travelItineraryCalculationService;
        _travelTravelBookingCalculationService = travelTravelBookingCalculationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTripCalculationService = travelTripCalculationService;
        _travelTripValidationService = travelTripValidationService;
        _sendGetTravelPolicyRules0 = _travelTripCalculationService.ExplainTripCalculationAsync;
    }
    /// <summary>
    /// Explains a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ExplainTravelPolicyDecisionResponse> ExplainTravelPolicyDecision(ApiTravel.ExplainTravelPolicyDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainTravelPolicyDecision for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SimulateItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.TravelPolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _travelItineraryCalculationService.SimulateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.GetTravelPolicyRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicyId = (request.TravelPolicy?.TravelPolicyId ?? ""),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelPolicyCalculationService.GetTravelPolicyRatesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiSearch.ExportSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.ExportSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply0.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream2 = _searchSearchIndexQueryService.ExportSearchIndexes(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportSearchIndexes frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiMaintenance.ResolveWorkOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await Workflow.SendAsync(input3, token, _maintenanceWorkOrderQueryService.ResolveWorkOrdersAsync);
        var response = new ApiTravel.ExplainTravelPolicyDecisionResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Parameters = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Priority = (reply1.TravelPolicy?.Priority ?? 0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ExplainTravelPolicyDecision", (request.TravelPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.GetTravelPolicyRulesResponse> GetTravelPolicyRules(ApiTravel.GetTravelPolicyRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTravelPolicyRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "GetTravelPolicyRules", "accepted", token);
        var input0 = new ApiTravel.ExplainTripCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTripCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                DueAt = (request.Context?.Deadline ?? new Timestamp()),
                Lines = { new ApiTravel.TripLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc0 = _sendGetTravelPolicyRules0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelPolicyRulesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Normalizes a travel policy.
    /// </summary>
    public override async Task<ApiTravel.NormalizeTravelPolicyResponse> NormalizeTravelPolicy(ApiTravel.NormalizeTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.EstimateItineraryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Lines = { new ApiTravel.ItineraryLine
                {
                    Tax = new ApiTravel.ItineraryLineTax
                    {
                        Amount = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
                        {
                            ApiTravel.TravelPolicy.ActionOneofCase.Discount => (request.TravelPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _travelItineraryCalculationService.EstimateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ExportDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ExportDevicePoliciesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.Itinerary?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _devicesDevicePolicyQueryService.ExportDevicePolicies(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDevicePolicies frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiTravel.CompareItineraryOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    Network = (reply0.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (reply0.Itinerary?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _travelItineraryCalculationService.CompareItineraryOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.NormalizeTravelPolicyResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Itinerary?.Audit?.UpdatedBy ?? ""),
                },
                Name = (request.TravelPolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Itinerary?.Counterparty?.DisplayName ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "NormalizeTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a travel policy.
    /// </summary>
    public override async Task<ApiTravel.PreviewTravelPolicyResponse> PreviewTravelPolicy(ApiTravel.PreviewTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiTravel.PreviewTravelPolicyRequest, ApiTravel.CalculateItineraryRequest>(value => new ApiTravel.CalculateItineraryRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (value.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    DisplayName = (value.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _travelItineraryCalculationService.CalculateItineraryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.CompareItineraryOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareItineraryOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Itinerary?.BillingAddress?.Region ?? ""),
                },
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Itinerary?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Card = new ApiTravel.ItineraryCardSettlement
                {
                    Token = (reply0.Itinerary ?? new ApiTravel.Itinerary()).PaymentCase switch
                    {
                        ApiTravel.Itinerary.SettlementOneofCase.Card => (reply0.Itinerary?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _travelItineraryCalculationService.CompareItineraryOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.PreviewTravelPolicyToEstimateTravelBooking2(reply0);
        using var rpc2 = _travelTravelBookingCalculationService.EstimateTravelBookingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.PreviewTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Itinerary?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.Itinerary?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply2.TravelBooking?.Counterparty?.Email ?? ""),
                },
                Status = (request.TravelPolicy?.Status ?? (ApiTravel.TravelPolicyStatus)0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "PreviewTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ScoreTravelPolicyResponse> ScoreTravelPolicy(ApiTravel.ScoreTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CompareSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareSignatureVersionsRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                Version = (request.TravelPolicy?.Version ?? 0L),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureHistoryService.CompareSignatureVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ScoreTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Name = (request.TravelPolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ScoreTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ValidateTravelPolicyResponse> ValidateTravelPolicy(ApiTravel.ValidateTravelPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateTravelPolicy for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.PreviewProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PreviewProblemRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _itsmProblemValidationService.PreviewProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ValidateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ValidateTripRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Problem?.Requester?.Phone ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Problem?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Problem?.Requester?.Email ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiTravel.ValidateTripRequest>();
        using var rpc1 = _travelTripValidationService.ValidateTripAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.ValidateTravelPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Problem?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Problem?.Customer?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
                Version = (request.TravelPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ValidateTravelPolicy", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
}
