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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Runs the long-lived workflow that takes a travel booking from start to finish.
/// </summary>
public sealed class TravelBookingWorkflowEndpoint : ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceBase
{
    private readonly ApiBudgeting.BudgetService.BudgetServiceClient _budgetingBudgetService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient _documentsDocumentTemplateWorkflowService;
    private readonly ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient _employeesEmployeeQueryService;
    private readonly ApiEmployees.EmployeeService.EmployeeServiceClient _employeesEmployeeService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ILogger<TravelBookingWorkflowEndpoint> _logger;
    private readonly ApiNotifications.NotificationService.NotificationServiceClient _notificationsNotificationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ITravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port _travelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port;
    private readonly ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient _travelItinerarySyncService;
    private readonly ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient _travelTravelBookingApprovalService;
    private readonly ApiTravel.TripService.TripServiceClient _travelTripService;
    private readonly ApiTravel.TripWorkflowService.TripWorkflowServiceClient _travelTripWorkflowService;

    public TravelBookingWorkflowEndpoint(
        ApiBudgeting.BudgetService.BudgetServiceClient budgetingBudgetService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.DocumentTemplateWorkflowService.DocumentTemplateWorkflowServiceClient documentsDocumentTemplateWorkflowService,
        ApiEmployees.EmployeeQueryService.EmployeeQueryServiceClient employeesEmployeeQueryService,
        ApiEmployees.EmployeeService.EmployeeServiceClient employeesEmployeeService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ILogger<TravelBookingWorkflowEndpoint> logger,
        ApiNotifications.NotificationService.NotificationServiceClient notificationsNotificationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ITravelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port travelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2PortAdapter,
        ApiTravel.ItinerarySyncService.ItinerarySyncServiceClient travelItinerarySyncService,
        ApiTravel.TravelBookingApprovalService.TravelBookingApprovalServiceClient travelTravelBookingApprovalService,
        ApiTravel.TripService.TripServiceClient travelTripService,
        ApiTravel.TripWorkflowService.TripWorkflowServiceClient travelTripWorkflowService)
    {
        _budgetingBudgetService = budgetingBudgetService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsDocumentTemplateWorkflowService = documentsDocumentTemplateWorkflowService;
        _employeesEmployeeQueryService = employeesEmployeeQueryService;
        _employeesEmployeeService = employeesEmployeeService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _logger = logger;
        _notificationsNotificationService = notificationsNotificationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _travelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port = travelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2PortAdapter;
        _travelItinerarySyncService = travelItinerarySyncService;
        _travelTravelBookingApprovalService = travelTravelBookingApprovalService;
        _travelTripService = travelTripService;
        _travelTripWorkflowService = travelTripWorkflowService;
    }
    /// <summary>
    /// Advances a travel booking.
    /// </summary>
    public override async Task<ApiTravel.AdvanceTravelBookingWorkflowResponse> AdvanceTravelBookingWorkflow(ApiTravel.AdvanceTravelBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceTravelBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ResolveItineraryConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ResolveItineraryConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Itinerary = new ApiTravel.Itinerary
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.TravelBooking?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiTravel.ItineraryBankSettlement
                {
                    Iban = (request.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (request.TravelBooking?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.TravelBooking?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        using var rpc0 = _travelItinerarySyncService.ResolveItineraryConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmployeeId = (request.TravelBooking?.Counterparty?.EmployeeId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeQueryService.SummarizeEmployeesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.AdvanceTravelBookingWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Itinerary?.Audit?.Etag ?? ""),
                },
                IssuedAt = (reply0.Itinerary?.IssuedAt ?? new Timestamp()),
                Number = (request.TravelBooking?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "AdvanceTravelBookingWorkflow", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists travel bookings a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListTravelBookingWorkflowsResponse> ListTravelBookingWorkflows(ApiTravel.ListTravelBookingWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListTravelBookingWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking", "batch",
            "ListTravelBookingWorkflows", "accepted", token);
        var response = new ApiTravel.ListTravelBookingWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ListTravelBookingWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a travel booking.
    /// </summary>
    public override async Task<ApiTravel.PauseTravelBookingWorkflowResponse> PauseTravelBookingWorkflow(ApiTravel.PauseTravelBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseTravelBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBudgeting.CreateBudgetRequest
        {
            Budget = new ApiBudgeting.Budget
            {
                Bank = new ApiBudgeting.BudgetBankFunding
                {
                    Iban = (request.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Bank => (request.TravelBooking?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _budgetingBudgetService.CreateBudgetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiTravel.ResumeTripWorkflowResponse();
        foreach (var item in (request.TravelBooking?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingLine>()))
        {
            var input1 = new ApiTravel.ResumeTripWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Trip = new ApiTravel.Trip
                {
                    Lines = { new ApiTravel.TripLine
                    {
                        Tax = new ApiTravel.TripLineTax
                        {
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc1 = _travelTripWorkflowService.ResumeTripWorkflowAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.TravelBooking?.Audit?.Etag ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiEmployees.PositionValidationService.PositionValidationServiceClient>("acme.employees.v1.PositionValidationService");
        using var rpc2 = named2.ScorePositionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLedger.ScoreLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Card = new ApiLedger.LedgerAccountCardFunding
                {
                    HolderName = (reply0.Budget ?? new ApiBudgeting.Budget()).PaymentCase switch
                    {
                        ApiBudgeting.Budget.FundingOneofCase.Card => (reply0.Budget?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _ledgerLedgerAccountValidationService.ScoreLedgerAccountAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.PauseTravelBookingWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply3.Result?.Status ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Budget?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Budget?.Contact?.Email ?? ""),
                },
                Metadata = { (request.TravelBooking?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "PauseTravelBookingWorkflow", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a travel booking.
    /// </summary>
    public override async Task<ApiTravel.ResumeTravelBookingWorkflowResponse> ResumeTravelBookingWorkflow(ApiTravel.ResumeTravelBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeTravelBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiTravel.SignalTripWorkflowResponse();
        foreach (var item in (request.TravelBooking?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingAttachment>()))
        {
            var input0 = new ApiTravel.SignalTripWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Trip = new ApiTravel.Trip
                {
                    Attachments = { new ApiTravel.TripAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc0 = _travelTripWorkflowService.SignalTripWorkflowAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var reply1 = new ApiDocuments.RetryDocumentTemplateWorkflowResponse();
        foreach (var item in (request.TravelBooking?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingAttachment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiDocuments.RetryDocumentTemplateWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DocumentTemplate = new ApiDocuments.DocumentTemplate
                {
                    Attachments = { new ApiDocuments.DocumentTemplateAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                        FileName = (item.FileName ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc1 = _documentsDocumentTemplateWorkflowService.RetryDocumentTemplateWorkflowAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiTravel.ResumeTravelBookingWorkflowResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                DueAt = (request.TravelBooking?.DueAt ?? new Timestamp()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "ResumeTravelBookingWorkflow", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a travel booking.
    /// </summary>
    public override async Task<ApiTravel.RetryTravelBookingWorkflowResponse> RetryTravelBookingWorkflow(ApiTravel.RetryTravelBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryTravelBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.TravelBooking?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingAttachment>()).Select(async item =>
        {
            var input0 = new ApiTravel.SubmitTravelBookingRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                TravelBooking = new ApiTravel.TravelBooking
                {
                    Attachments = { new ApiTravel.TravelBookingAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc0 = _travelTravelBookingApprovalService.SubmitTravelBookingAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiTravel.SubmitTravelBookingResponse();
        var reply1 = new ApiTravel.CommentTravelBookingApprovalResponse();
        foreach (var item in (request.TravelBooking?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingAttachment>()))
        {
            var input1 = new ApiTravel.CommentTravelBookingApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                TravelBooking = new ApiTravel.TravelBooking
                {
                    Attachments = { new ApiTravel.TravelBookingAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        ContentType = (item.ContentType ?? ""),
                        FileName = (item.FileName ?? ""),
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc1 = _travelTravelBookingApprovalService.CommentTravelBookingApprovalAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiNotifications.CreateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.TravelBooking?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.TravelBooking?.Counterparty?.Email ?? ""),
                },
            },
        };
        var reply2 = await SendRetryTravelBookingWorkflow2Async(input2, token);
        var batch3 = (request.TravelBooking?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TravelBookingLine>()).Select(async item =>
        {
            var input3 = new ApiTravel.PauseTripWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Trip = new ApiTravel.Trip
                {
                    Lines = { new ApiTravel.TripLine
                    {
                        Tax = new ApiTravel.TripLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc3 = _travelTripWorkflowService.PauseTripWorkflowAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            return reply3;
        }).ToArray();
        var replies3 = await Task.WhenAll(batch3);
        var reply3 = replies3.LastOrDefault() ?? new ApiTravel.PauseTripWorkflowResponse();
        var response = new ApiTravel.RetryTravelBookingWorkflowResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Notification?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = (request.TravelBooking?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Number = (reply2.Notification?.Customer?.Number ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "RetryTravelBookingWorkflow", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a travel booking.
    /// </summary>
    public override async Task<ApiTravel.SignalTravelBookingWorkflowResponse> SignalTravelBookingWorkflow(ApiTravel.SignalTravelBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalTravelBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.QuoteEmploymentContractRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.TravelBooking?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.TravelBooking?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (request.TravelBooking?.Counterparty?.EmployeeId ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiTravel.GetTravelBookingApprovalStatusRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBookingId = (request.TravelBooking?.TravelBookingId ?? ""),
        };
        using var rpc1 = _travelTravelBookingApprovalService.GetTravelBookingApprovalStatusAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiTravel.SignalTravelBookingWorkflowResponse
        {
            TravelBooking = new ApiTravel.TravelBooking
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.TravelBooking?.Counterparty?.DisplayName ?? ""),
                },
                IssuedAt = (request.TravelBooking?.IssuedAt ?? new Timestamp()),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "SignalTravelBookingWorkflow", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a travel booking.
    /// </summary>
    public override async Task<ApiTravel.StartTravelBookingWorkflowResponse> StartTravelBookingWorkflow(ApiTravel.StartTravelBookingWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartTravelBookingWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelBooking is { } record)
        {
            var snapshot = TravelBookingMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.GetEmployeeRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.EmployeeId = (request.TravelBooking?.Counterparty?.EmployeeId ?? "");
        using var rpc0 = _employeesEmployeeService.GetEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.LintEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.Employee?.PrimaryAddress?.Region ?? ""),
                },
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    HolderName = (request.TravelBooking ?? new ApiTravel.TravelBooking()).PaymentCase switch
                    {
                        ApiTravel.TravelBooking.SettlementOneofCase.Card => (request.TravelBooking?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractValidationService.LintEmploymentContractsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTravel.CreateTripRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTripService.CreateTripAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelBooking?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Employee?.RelationshipManager?.UserId ?? ""),
                Region = (reply0.Employee?.PrimaryAddress?.Region ?? ""),
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateNotificationService.ListDocumentTemplateSubscriptionsAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.StartTravelBookingWorkflowResponse
        {
            Outcome = new ApiTravel.StartTravelBookingWorkflowResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
            },
            TravelBooking = new ApiTravel.TravelBooking
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Employee?.RelationshipManager?.DisplayName ?? ""),
                },
                IssuedAt = (request.TravelBooking?.IssuedAt ?? new Timestamp()),
                Notes = (reply2.Trip?.Notes ?? ""),
                TravelBookingId = (request.TravelBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelBooking?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_booking",
            "audit", "StartTravelBookingWorkflow", (request.TravelBooking?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiNotifications.CreateNotificationResponse> SendRetryTravelBookingWorkflow2Async(ApiNotifications.CreateNotificationRequest request, CancellationToken token)
    {
        return await _travelBookingWorkflowEndpointSendRetryTravelBookingWorkflow2Port.SendAsync(request, token);
    }
}
