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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Manages trips: creates, reads, changes, and lists them.
/// </summary>
public sealed class TripEndpoint : ApiTravel.TripService.TripServiceBase
{
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient _devicesManagedDeviceService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient _documentsDocumentTemplateSyncService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiHelpdesk.TicketQueryService.TicketQueryServiceClient _helpdeskTicketQueryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentService.IncidentServiceClient _itsmIncidentService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ILogger<TripEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient _travelTravelBookingValidationService;
    private readonly ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient _travelTravelPolicyCalculationService;
    private readonly ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient _travelTravelPolicyValidationService;
    private readonly ApiTravel.TripCalculationService.TripCalculationServiceClient _travelTripCalculationService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TripEndpoint(
        NotificationBus bus,
        ApiDevices.ManagedDeviceService.ManagedDeviceServiceClient devicesManagedDeviceService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentTemplateSyncService.DocumentTemplateSyncServiceClient documentsDocumentTemplateSyncService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiHelpdesk.TicketQueryService.TicketQueryServiceClient helpdeskTicketQueryService,
        CallInvoker invoker,
        ApiItsm.IncidentService.IncidentServiceClient itsmIncidentService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ILogger<TripEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.TravelBookingValidationService.TravelBookingValidationServiceClient travelTravelBookingValidationService,
        ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient travelTravelPolicyCalculationService,
        ApiTravel.TravelPolicyValidationService.TravelPolicyValidationServiceClient travelTravelPolicyValidationService,
        ApiTravel.TripCalculationService.TripCalculationServiceClient travelTripCalculationService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _bus = bus;
        _devicesManagedDeviceService = devicesManagedDeviceService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentTemplateSyncService = documentsDocumentTemplateSyncService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _helpdeskTicketQueryService = helpdeskTicketQueryService;
        _invoker = invoker;
        _itsmIncidentService = itsmIncidentService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _travelTravelBookingValidationService = travelTravelBookingValidationService;
        _travelTravelPolicyCalculationService = travelTravelPolicyCalculationService;
        _travelTravelPolicyValidationService = travelTravelPolicyValidationService;
        _travelTripCalculationService = travelTripCalculationService;
        _travelTripValidationService = travelTripValidationService;
    }
    /// <summary>
    /// Takes a stream of trips and answers once at the end.
    /// </summary>
    public override async Task<ApiTravel.BatchCreateTripsResponse> BatchCreateTrips(IAsyncStreamReader<ApiTravel.BatchCreateTripsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiTravel.BatchCreateTripsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateTrips for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiDocuments.ScheduleDocumentExportRequest
            {
                Annotations = { request.ChunkCase switch
                {
                    ApiTravel.BatchCreateTripsRequest.ChunkOneofCase.Record => (request.Record?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()),
                    _ => new Google.Protobuf.Collections.MapField<string, string>()
                } },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
                {
                    Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Format = request.ChunkCase switch
                    {
                        ApiTravel.BatchCreateTripsRequest.ChunkOneofCase.Header => (request.Header?.Format ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc0 = _documentsDocumentExportService.ScheduleDocumentExportAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiHelpdesk.SummarizeTicketsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiHelpdesk.SummarizeTicketsRequest.Types.Details
                {
                    Period = (reply0.Period ?? new ApiCommon.TimeWindow()),
                },
            };
            using var rpc1 = _helpdeskTicketQueryService.SummarizeTicketsAsync(input1, cancellationToken: token);
            var pending1 = rpc1.ResponseAsync;
            var input2 = new ApiTravel.CheckTravelBookingEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                TravelBooking = new ApiTravel.TravelBooking
                {
                    Bank = new ApiTravel.TravelBookingBankSettlement
                    {
                        Bic = (request.Record?.Bank?.Bic ?? ""),
                    },
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (request.Record?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
            };
            using var rpc2 = _travelTravelBookingValidationService.CheckTravelBookingEligibilityAsync(input2, cancellationToken: token);
            var pending2 = rpc2.ResponseAsync;
            await Task.WhenAll(pending1, pending2);
            var reply1 = await pending1;
            var reply2 = await pending2;
            var response = new ApiTravel.BatchCreateTripsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Message = (reply1.Result?.Message ?? ""),
                    ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Status = (reply1.Result?.Status ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.VoucherCode ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "trip",
                "audit", "BatchCreateTrips", (request.Record?.VoucherCode ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Creates a trip.
    /// </summary>
    public override async Task<ApiTravel.CloneTripResponse> CloneTrip(ApiTravel.CloneTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.QuoteTravelPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.QuoteTravelPolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Trip?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (request.Trip?.BillingAddress?.Recipient ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Trip?.Audit?.Etag ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiTravel.TravelPolicyCalculationService.TravelPolicyCalculationServiceClient>("acme.travel.v1.TravelPolicyCalculationService");
        using var rpc0 = named0.QuoteTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainTravelPolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Trip?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiTravel.ExplainTravelPolicyCalculationRequest, ApiTravel.ExplainTravelPolicyCalculationResponse>(
            MethodType.Unary, "acme.travel.v1" + ".TravelPolicyCalculationService", "ExplainTravelPolicyCalculation",
            Wire.Marshaller<ApiTravel.ExplainTravelPolicyCalculationRequest>(), Wire.Marshaller<ApiTravel.ExplainTravelPolicyCalculationResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CloneTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.TravelPolicy?.Owner?.DisplayName ?? ""),
                },
                Metadata = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (request.Trip?.Status ?? (ApiTravel.TripStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CloneTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a trip.
    /// </summary>
    public override async Task<ApiTravel.CreateTripResponse> CreateTrip(ApiTravel.CreateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.GetTripRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TripId = (request.Trip?.TripId ?? ""),
        };
        var command0 = new DispatchCommand<ApiTravel.GetTripRatesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _travelTripCalculationService.GetTripRatesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.CreateTripResponse
        {
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply0.Trip?.Counterparty?.EmployeeId ?? ""),
                },
                DueAt = (request.Trip?.DueAt ?? new Timestamp()),
                IssuedAt = (request.Trip?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CreateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a trip.
    /// </summary>
    public override async Task<ApiTravel.DeleteTripResponse> DeleteTrip(ApiTravel.DeleteTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "DeleteTrip", "accepted", token);
        var profile0 = new MappingProfile<ApiTravel.DeleteTripRequest, ApiTravel.QuoteTravelPolicyRequest>(value => new ApiTravel.QuoteTravelPolicyRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (value.Etag ?? ""),
                },
                Block = new ApiTravel.TravelPolicyBlockAction
                {
                    Reason = (value.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                    UserId = (value.Context?.Actor?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _travelTravelPolicyCalculationService.QuoteTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTravel.QuoteTravelPolicyRequest>("acme.travel.v1.TripService/DeleteTrip/0", input0.Clone()), token);
        var input1 = new ApiItsm.ListIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.TravelPolicy?.Owner?.UserId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmIncidentService.ListIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.DeleteTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TripId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "DeleteTrip", (request.TripId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.GetTripResponse> GetTrip(ApiTravel.GetTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTrip for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "GetTrip", "accepted", token);
        var response = new ApiTravel.GetTripResponse
        {
            Trip = new ApiTravel.Trip
            {
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListTripsResponse> ListTrips(ApiTravel.ListTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "ListTrips", "accepted", token);
        var input0 = new ApiDocuments.ShareDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareDocumentFolderExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentFolderExportService.ShareDocumentFolderExportAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.ListManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListManagedDevicesRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
            },
        };
        using var rpc1 = _devicesManagedDeviceService.ListManagedDevicesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTravel.ListTripsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply1.PageInfo?.NextPageToken ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Trips = { new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.DocumentFolder?.Location?.CountryCode ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ListTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists trips a page at a time.
    /// </summary>
    public override async Task<ApiTravel.SearchTripsResponse> SearchTrips(ApiTravel.SearchTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "SearchTrips", "accepted", token);
        var input0 = new ApiTravel.NormalizeTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.NormalizeTripRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _travelTripValidationService.NormalizeTripAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainTravelPolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTravelPolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Trip?.Counterparty?.Email ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Trip?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiTravel.ExplainTravelPolicyDecisionRequest>();
        using var rpc1 = _travelTravelPolicyValidationService.ExplainTravelPolicyDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.SearchTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trips = { new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Trip?.Audit?.UpdatedBy ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Description = (reply1.TravelPolicy?.Description ?? ""),
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SearchTrips", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a trip.
    /// </summary>
    public override async Task<ApiTravel.UpdateTripResponse> UpdateTrip(ApiTravel.UpdateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PreviewSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PreviewSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                    Number = (request.Trip?.Number ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureValidationService.PreviewSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.LintIncidentsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Incident = new ApiItsm.Incident
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.Trip?.Audit?.Etag ?? ""),
            },
        };
        using var rpc1 = _itsmIncidentValidationService.LintIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiTravel.ValidateTravelBookingResponse();
        foreach (var item in (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()))
        {
            var input2 = new ApiTravel.ValidateTravelBookingRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                TravelBooking = new ApiTravel.TravelBooking
                {
                    Lines = { new ApiTravel.TravelBookingLine
                    {
                        Tax = new ApiTravel.TravelBookingLineTax
                        {
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Jurisdiction = (item.Tax?.Jurisdiction ?? ""),
                        },
                    } },
                },
            };
            using var rpc2 = _travelTravelBookingValidationService.ValidateTravelBookingAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiTravel.UpdateTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = (request.Trip?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.Incident?.Assignee?.DisplayName ?? ""),
                },
                DueAt = (request.Trip?.DueAt ?? new Timestamp()),
                Number = (reply0.Signature?.Subject?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "UpdateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of trips as they happen.
    /// </summary>
    public override async Task WatchTrips(ApiTravel.WatchTripsRequest request, IServerStreamWriter<ApiTravel.WatchTripsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchTrips for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", "batch",
            "WatchTrips", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.PushDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
            {
                Filters = { (reply0.DocumentTemplate?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentExportService.ScheduleDocumentExportAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.WatchTripsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "WatchTrips", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
