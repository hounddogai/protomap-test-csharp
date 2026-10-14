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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Routes travel bookings through approval, with escalation and delegation.
/// </summary>
public sealed class TravelBookingApprovalEndpoint : ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceBase
{
    private readonly ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient _documentsDocumentFolderHistoryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<TravelBookingApprovalEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiTravel.ScheduleItineraryExpiryRequest, CallOptions, AsyncUnaryCall<ApiTravel.ScheduleItineraryExpiryResponse>> _sendGetTravelBookingApprovalStatus0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient _travelItineraryLifecycleService;
    private readonly ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient _travelItineraryReportingService;
    private readonly ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient _travelTravelBookingLifecycleService;
    private readonly ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient _travelTravelBookingReportingService;
    private readonly ApiTravel.TripValidationService.TripValidationServiceClient _travelTripValidationService;

    public TravelBookingApprovalEndpoint(
        ApiDocuments.DocumentFolderHistoryService.DocumentFolderHistoryServiceClient documentsDocumentFolderHistoryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        CallInvoker invoker,
        ILogger<TravelBookingApprovalEndpoint> logger,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTravel.ItineraryLifecycleService.ItineraryLifecycleServiceClient travelItineraryLifecycleService,
        ApiTravel.ItineraryReportingService.ItineraryReportingServiceClient travelItineraryReportingService,
        ApiTravel.TravelBookingLifecycleService.TravelBookingLifecycleServiceClient travelTravelBookingLifecycleService,
        ApiTravel.TravelBookingReportingService.TravelBookingReportingServiceClient travelTravelBookingReportingService,
        ApiTravel.TripValidationService.TripValidationServiceClient travelTripValidationService)
    {
        _documentsDocumentFolderHistoryService = documentsDocumentFolderHistoryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _invoker = invoker;
        _logger = logger;
        _repository = repository;
        _settings = settings;
        _travelItineraryLifecycleService = travelItineraryLifecycleService;
        _travelItineraryReportingService = travelItineraryReportingService;
        _travelTravelBookingLifecycleService = travelTravelBookingLifecycleService;
        _travelTravelBookingReportingService = travelTravelBookingReportingService;
        _travelTripValidationService = travelTripValidationService;
        _sendGetTravelBookingApprovalStatus0 = _travelItineraryLifecycleService.ScheduleItineraryExpiryAsync;
    }
    /// <summary>
    /// Approves a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ApproveTravelBookingResponse> ApproveTravelBooking(ApiTravel.ApproveTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ApproveTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RecordDocumentFolderVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordDocumentFolderVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (request.TravelBooking?.Audit?.CreatedBy ?? ""),
                    },
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.TravelBooking?.BillingAddress?.Recipient ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Approver?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderHistoryService.RecordDocumentFolderVersionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiEmployees.ListEmployeesResponse();
        foreach (var item in (request.TravelBooking?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingLine>()))
        {
            var input1 = new ApiEmployees.ListEmployeesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.LineId ?? ""),
                    Region = (reply0.DocumentFolder?.Location?.Region ?? ""),
                },
            };
            using var rpc1 = _employeesEmployeeService.ListEmployeesAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiTravel.ApproveTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Number = (request.TravelBooking?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ApproveTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a travel booking.
    /// </summary>
    public override async Task<ApiTravel.CommentTravelBookingApprovalResponse> CommentTravelBookingApproval(ApiTravel.CommentTravelBookingApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CommentTravelBookingApproval for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ScheduleTravelBookingExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.TravelBooking?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.ScheduleTravelBookingExpiryRequest>(json0);
        using var rpc0 = _travelTravelBookingLifecycleService.ScheduleTravelBookingExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ExplainTripDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ExplainTripDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Bank = new ApiTravel.TripBankSettlement
                {
                    Bic = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply0.TravelBooking?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                    Iban = (reply0.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (reply0.TravelBooking?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _travelTripValidationService.ExplainTripDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.CommentTravelBookingApprovalResponse
        {
            Outcome = new ApiTravel.CommentTravelBookingApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "CommentTravelBookingApproval", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a travel booking.
    /// </summary>
    public override async Task<ApiTravel.EscalateTravelBookingResponse> EscalateTravelBooking(ApiTravel.EscalateTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EscalateTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.TravelBooking?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.TravelBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.TravelBooking?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesPositionValidationService.PreviewPositionAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiTravel.GenerateTravelBookingReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply1.Position?.Block?.Reason ?? ""),
        };
        using var rpc2 = _travelTravelBookingReportingService.GenerateTravelBookingReportAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiTravel.ValidateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Card = new ApiTravel.TripCardSettlement
                {
                    Token = (request.TravelBooking?.Card?.Token ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Tax = new ApiTravel.TripLineTax
                    {
                        Amount = (reply1.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                    },
                } },
            },
        };
        using var rpc3 = _travelTripValidationService.ValidateTripAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiTravel.EscalateTravelBookingResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = (request.TravelBooking?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply3.Trip?.Counterparty?.DisplayName ?? ""),
                },
                Metadata = { (reply3.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "EscalateTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel booking.
    /// </summary>
    public override async Task<ApiTravel.GetTravelBookingApprovalStatusResponse> GetTravelBookingApprovalStatus(ApiTravel.GetTravelBookingApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelBookingId, "travel_booking_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTravelBookingApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", request.TravelBookingId,
            "GetTravelBookingApprovalStatus", "accepted", token);
        var input0 = new ApiTravel.ScheduleItineraryExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ScheduleItineraryExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Counterparty = new ApiTravel.TravelBookingRef
                {
                    TravelBookingId = (request.TravelBookingId ?? ""),
                },
            },
        };
        using var rpc0 = _sendGetTravelBookingApprovalStatus0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTravelBookingApprovalStatusResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Itinerary?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists travel bookings a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListPendingTravelBookingsResponse> ListPendingTravelBookings(ApiTravel.ListPendingTravelBookingsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListPendingTravelBookings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "ListPendingTravelBookings", "accepted", token);
        var input0 = new ApiTravel.StreamItineraryMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.StreamItineraryMetricsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _travelItineraryReportingService.StreamItineraryMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamItineraryMetrics frame {Type}", change0.GetType().Name);
        }
        var response = new ApiTravel.ListPendingTravelBookingsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ListPendingTravelBookings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a travel booking.
    /// </summary>
    public override async Task<ApiTravel.RejectTravelBookingResponse> RejectTravelBooking(ApiTravel.RejectTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RejectTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.TravelBooking?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiEmployees.NormalizePositionRequest, ApiEmployees.NormalizePositionResponse>(
            MethodType.Unary, "acme.employees.v1" + ".PositionValidationService", "NormalizePosition",
            Wire.Marshaller<ApiEmployees.NormalizePositionRequest>(), Wire.Marshaller<ApiEmployees.NormalizePositionResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.RejectTravelBookingResponse
        {
            Outcome = new ApiTravel.RejectTravelBookingResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = (request.TravelBooking?.BillingAddress ?? new ApiCommon.PostalAddress()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "RejectTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a travel booking.
    /// </summary>
    public override async Task<ApiTravel.SubmitTravelBookingResponse> SubmitTravelBooking(ApiTravel.SubmitTravelBookingRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SubmitTravelBooking for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ScorePositionRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.ScorePositionRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesPositionValidationService.ScorePositionAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizeEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.TravelBooking?.Counterparty?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (reply0.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _employeesEmploymentContractValidationService.NormalizeEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.TerminateTravelBookingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.TerminateTravelBookingRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.TravelBooking?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.EmploymentContract?.Counterparty?.Email ?? ""),
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            TravelBookingId = (request.TravelBookingId ?? ""),
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _travelTravelBookingLifecycleService.TerminateTravelBookingAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.SubmitTravelBookingResponse
        {
            Outcome = new ApiTravel.SubmitTravelBookingResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Position?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply2.TravelBooking?.Counterparty?.DisplayName ?? ""),
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
                IssuedAt = (request.TravelBooking?.IssuedAt ?? new Timestamp()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "SubmitTravelBooking", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
}
