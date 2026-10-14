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
using ApiAppointments = Acme.Appointments.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Checks clock events against the rules before anything acts on them.
/// </summary>
public sealed class ClockEventValidationEndpoint : ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceBase
{
    private readonly ApiAppointments.AppointmentService.AppointmentServiceClient _appointmentsAppointmentService;
    private readonly ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient _authorizationAccessPolicyService;
    private readonly NotificationBus _bus;
    private readonly IClockEventValidationEndpointSendPreviewClockEvent1Port _clockEventValidationEndpointSendPreviewClockEvent1Port;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ILogger<ClockEventValidationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiAuthorization.UpdateAccessPolicyRequest, CallOptions, AsyncUnaryCall<ApiAuthorization.UpdateAccessPolicyResponse>> _sendExplainClockEventDecision0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;
    private readonly ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient _travelTravelBookingQueryService;

    public ClockEventValidationEndpoint(
        ApiAppointments.AppointmentService.AppointmentServiceClient appointmentsAppointmentService,
        ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient authorizationAccessPolicyService,
        NotificationBus bus,
        IClockEventValidationEndpointSendPreviewClockEvent1Port clockEventValidationEndpointSendPreviewClockEvent1PortAdapter,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ILogger<ClockEventValidationEndpoint> logger,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService,
        ApiTravel.TravelBookingQueryService.TravelBookingQueryServiceClient travelTravelBookingQueryService)
    {
        _appointmentsAppointmentService = appointmentsAppointmentService;
        _authorizationAccessPolicyService = authorizationAccessPolicyService;
        _bus = bus;
        _clockEventValidationEndpointSendPreviewClockEvent1Port = clockEventValidationEndpointSendPreviewClockEvent1PortAdapter;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _logger = logger;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
        _travelTravelBookingQueryService = travelTravelBookingQueryService;
        _sendExplainClockEventDecision0 = _authorizationAccessPolicyService.UpdateAccessPolicyAsync;
    }
    /// <summary>
    /// Checks a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.CheckClockEventEligibilityResponse> CheckClockEventEligibility(ApiTimekeeping.CheckClockEventEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CheckClockEventEligibility for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAppointments.CreateAppointmentRequest
        {
            Appointment = new ApiAppointments.Appointment
            {
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _appointmentsAppointmentService.CreateAppointmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.RecalculateOvertimesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Appointment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _timekeepingOvertimeCalculationService.RecalculateOvertimesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiTimekeeping.GetClockEventRatesRequest
        {
            ClockEventId = (request.ClockEvent?.ClockEventId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _timekeepingClockEventCalculationService.GetClockEventRatesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiDocuments.SendDocumentTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Appointment?.Organizer?.Email ?? ""),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc3 = _documentsDocumentNotificationService.SendDocumentTestNoticeAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.CheckClockEventEligibilityResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                OccurredAt = (reply2.ClockEvent?.OccurredAt ?? new Timestamp()),
                Source = (request.ClockEvent?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "CheckClockEventEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.ExplainClockEventDecisionResponse> ExplainClockEventDecision(ApiTimekeeping.ExplainClockEventDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainClockEventDecision for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.UpdateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendExplainClockEventDecision0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.ExplainClockEventDecisionResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Source = (request.ClockEvent?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ExplainClockEventDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.GetClockEventRulesResponse> GetClockEventRules(ApiTimekeeping.GetClockEventRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ClockEventId, "clock_event_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetClockEventRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "clock_event", request.ClockEventId,
            "GetClockEventRules", "accepted", token);
        var response = new ApiTimekeeping.GetClockEventRulesResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.LintClockEventsResponse> LintClockEvents(ApiTimekeeping.LintClockEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LintClockEvents for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.CompareOvertimeOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Block = new ApiTimekeeping.OvertimeBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient>("acme.timekeeping.v1.OvertimeCalculationService");
        using var rpc0 = named0.CompareOvertimeOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.LintClockEventsResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Source = (request.ClockEvent?.Source ?? ""),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (reply0.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "LintClockEvents", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.NormalizeClockEventResponse> NormalizeClockEvent(ApiTimekeeping.NormalizeClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling NormalizeClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.QuoteClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.ClockEvent?.OccurredAt ?? new Timestamp()),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    TimesheetId = (request.ClockEvent?.Subject?.TimesheetId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _timekeepingClockEventCalculationService.QuoteClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.GetClockEventRatesRequest
        {
            ClockEventId = (request.ClockEvent?.ClockEventId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command1 = new DispatchCommand<ApiTimekeeping.GetClockEventRatesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _timekeepingClockEventCalculationService.GetClockEventRatesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()).Select(async item =>
        {
            var input2 = new ApiDocuments.PreviewSignatureRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDocuments.SignatureMeasurement
                    {
                        Metric = (item.Metric ?? ""),
                        Unit = (item.Unit ?? ""),
                    } },
                },
            };
            using var rpc2 = _documentsSignatureValidationService.PreviewSignatureAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiDocuments.PreviewSignatureResponse();
        var input3 = new ApiTimekeeping.ValidateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ValidateTimesheetRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                },
                Number = (request.ClockEvent?.Subject?.Number ?? ""),
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _timekeepingTimesheetValidationService.ValidateTimesheetAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiTimekeeping.ValidateTimesheetRequest>("acme.timekeeping.v1.ClockEventValidationService/NormalizeClockEvent/3", input3.Clone()), token);
        var response = new ApiTimekeeping.NormalizeClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Sequence = (reply1.ClockEvent?.Sequence ?? 0L),
                Source = (request.ClockEvent?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "NormalizeClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.PreviewClockEventResponse> PreviewClockEvent(ApiTimekeeping.PreviewClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PreviewClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.PreviewClockEventToCalculateOvertime0();
        using var rpc0 = _timekeepingOvertimeCalculationService.CalculateOvertimeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.PreviewTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Overtime?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await SendPreviewClockEvent1Async(input1, token);
        var input2 = new ApiDevices.SimulateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Block = new ApiDevices.DevicePolicyBlockAction
                {
                    NotifyEmail = (reply0.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Block => (reply0.Overtime?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _devicesDevicePolicyCalculationService.SimulateDevicePolicyAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiTimekeeping.ExplainOvertimeCalculationResponse();
        foreach (var item in (request.ClockEvent?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.ClockEventMeasurement>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiTimekeeping.ExplainOvertimeCalculationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Overtime = new ApiTimekeeping.Overtime
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.ObservedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        UserId = (reply2.DevicePolicy?.Owner?.UserId ?? ""),
                    },
                },
            };
            using var rpc3 = _timekeepingOvertimeCalculationService.ExplainOvertimeCalculationAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiTimekeeping.PreviewClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Sequence = (request.ClockEvent?.Sequence ?? 0L),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    Number = (reply1.Timesheet?.Number ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "PreviewClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.ScoreClockEventResponse> ScoreClockEvent(ApiTimekeeping.ScoreClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ScoreClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.PreviewTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Number = (request.ClockEvent?.Subject?.Number ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTimekeeping.PreviewTimesheetRequest>(json0);
        using var rpc0 = _timekeepingTimesheetValidationService.PreviewTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.QuoteOvertimeRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiTimekeeping.QuoteOvertimeRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Recipient = (request.Details?.Destination?.Recipient ?? ""),
            },
        };
        input1.Overtime = new ApiTimekeeping.Overtime
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (reply0.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _timekeepingOvertimeCalculationService.QuoteOvertimeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ScoreClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Attributes = { (request.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ClockEventId = (request.ClockEventId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ScoreClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a clock event.
    /// </summary>
    public override async Task<ApiTimekeeping.ValidateClockEventResponse> ValidateClockEvent(ApiTimekeeping.ValidateClockEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ValidateClockEvent for {Tenant}", requestContext.TenantId);
        if (request.ClockEvent is { } record)
        {
            var snapshot = ClockEventMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.EstimateOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingOvertimeCalculationService.EstimateOvertimeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.SuggestTravelBookingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTravel.SuggestTravelBookingsRequest.Types.Filter
            {
                CreatedAfter = (request.ClockEvent?.OccurredAt ?? new Timestamp()),
                Labels = { (reply0.Overtime?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ClockEvent?.ClockEventId ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelBookingQueryService.SuggestTravelBookingsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.ValidateClockEventResponse
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                ClockEventId = (request.ClockEventId ?? ""),
                Kind = (request.ClockEvent?.Kind ?? ""),
            },
            Outcome = new ApiTimekeeping.ValidateClockEventResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "clock_event",
            "audit", "ValidateClockEvent", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiTimekeeping.PreviewTimesheetResponse> SendPreviewClockEvent1Async(ApiTimekeeping.PreviewTimesheetRequest request, CancellationToken token)
    {
        return await _clockEventValidationEndpointSendPreviewClockEvent1Port.SendAsync(request, token);
    }
}
