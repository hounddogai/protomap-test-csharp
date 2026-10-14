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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiTravel = Acme.Travel.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Timekeeping;

public sealed partial class TimesheetWorkflowEndpoint
{
    /// <summary>
    /// Advances a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.AdvanceTimesheetWorkflowResponse> AdvanceTimesheetWorkflow(ApiTimekeeping.AdvanceTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiAuthorization.ListAccessPoliciesResponse();
        foreach (var item in (request.Timesheet?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTimekeeping.TimesheetLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiAuthorization.ListAccessPoliciesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiAuthorization.ListAccessPoliciesRequest.Types.Filter
                {
                    Labels = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.LineId ?? ""),
                    Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
                },
            };
            using var rpc0 = _authorizationAccessPolicyService.ListAccessPoliciesAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiTimekeeping.ValidateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Bank = new ApiTimekeeping.TimesheetBankSettlement
                {
                    AccountHolder = (request.Timesheet ?? new ApiTimekeeping.Timesheet()).PaymentCase switch
                    {
                        ApiTimekeeping.Timesheet.SettlementOneofCase.Bank => (request.Timesheet?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                    Bic = (request.Timesheet ?? new ApiTimekeeping.Timesheet()).PaymentCase switch
                    {
                        ApiTimekeeping.Timesheet.SettlementOneofCase.Bank => (request.Timesheet?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Timesheet?.BillingAddress?.CountryCode ?? ""),
                    Lines = { (request.Timesheet?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc1 = _timekeepingTimesheetValidationService.ValidateTimesheetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.AdvanceTimesheetWorkflowResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = (request.Timesheet?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                Status = (reply1.Timesheet?.Status ?? (ApiTimekeeping.TimesheetStatus)0),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "AdvanceTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.CancelTimesheetWorkflowResponse> CancelTimesheetWorkflow(ApiTimekeeping.CancelTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.RetryTravelBookingWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TravelBooking = new ApiTravel.TravelBooking
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Timesheet?.Counterparty?.Email ?? ""),
                    EmployeeId = (request.Timesheet?.Counterparty?.EmployeeId ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.RetryTravelBookingWorkflowRequest>(json0);
        using var rpc0 = _travelTravelBookingWorkflowService.RetryTravelBookingWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.CancelTimesheetWorkflowResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = (reply0.TravelBooking?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                Number = (request.Timesheet?.Number ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "CancelTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.InspectTimesheetWorkflowResponse> InspectTimesheetWorkflow(ApiTimekeeping.InspectTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TimesheetId, "timesheet_id");
        _logger.LogInformation("Handling InspectTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "InspectTimesheetWorkflow", "accepted", token);
        var response = new ApiTimekeeping.InspectTimesheetWorkflowResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists timesheets a page at a time.
    /// </summary>
    public override async Task<ApiTimekeeping.ListTimesheetWorkflowsResponse> ListTimesheetWorkflows(ApiTimekeeping.ListTimesheetWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListTimesheetWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "ListTimesheetWorkflows", "accepted", token);
        var input0 = new ApiTimekeeping.RejectClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.RejectClockEventRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventApprovalService.RejectClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.PauseSlaPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ClockEvent?.Subject?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskSlaPolicyWorkflowService.PauseSlaPolicyWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ListTimesheetWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Timesheets = { new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.SlaPolicy?.Audit?.Etag ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ListTimesheetWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.PauseTimesheetWorkflowResponse> PauseTimesheetWorkflow(ApiTimekeeping.PauseTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.SummarizeClockEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _timekeepingClockEventQueryService.SummarizeClockEventsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiTreasury.CloneCashPositionRequest
        {
            CashPosition = new ApiTreasury.CashPosition
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Timesheet?.Counterparty?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _treasuryCashPositionService.CloneCashPositionAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployees.WatchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                PreferredChannel = (reply1.CashPosition?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var stream2 = _employeesEmployeeService.WatchEmployees(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchEmployees frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiLearning.RecallCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Timesheet?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Timesheet?.Audit?.UpdatedBy ?? ""),
                },
            },
            Details = new ApiLearning.RecallCourseRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Timesheet?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _sendPauseTimesheetWorkflow3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.PauseTimesheetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.CashPosition?.Audit?.CreatedBy ?? ""),
                },
                Metadata = { (request.Timesheet?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "PauseTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.ResumeTimesheetWorkflowResponse> ResumeTimesheetWorkflow(ApiTimekeeping.ResumeTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.BatchGetAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiAuthorization.AccessPolicyService.AccessPolicyServiceClient>("acme.authorization.v1.AccessPolicyService");
        using var rpc0 = named0.BatchGetAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiAuthorization.BatchGetAccessPoliciesRequest>("acme.timekeeping.v1.TimesheetWorkflowService/ResumeTimesheetWorkflow/0", input0.Clone()), token);
        var input1 = new ApiTravel.ScheduleItineraryExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Itinerary = new ApiTravel.Itinerary
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Timesheet?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiTravel.ScheduleItineraryExpiryRequest, ApiTravel.ScheduleItineraryExpiryResponse>(
            MethodType.Unary, "acme.travel.v1" + ".ItineraryLifecycleService", "ScheduleItineraryExpiry",
            Wire.Marshaller<ApiTravel.ScheduleItineraryExpiryRequest>(), Wire.Marshaller<ApiTravel.ScheduleItineraryExpiryResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.ResumeTimesheetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Notes = (request.Timesheet?.Notes ?? ""),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "ResumeTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.RetryTimesheetWorkflowResponse> RetryTimesheetWorkflow(ApiTimekeeping.RetryTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Timesheet?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.CreateEmployeeRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.RetryTimesheetWorkflowResponse
        {
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Employee?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.Employee?.Contact?.Email ?? ""),
                },
                TimesheetId = (request.TimesheetId ?? ""),
                Total = (request.Timesheet?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "RetryTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.SignalTimesheetWorkflowResponse> SignalTimesheetWorkflow(ApiTimekeeping.SignalTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiTimekeeping.SignalTimesheetWorkflowRequest, ApiTimekeeping.SimulateOvertimeRequest>(value => new ApiTimekeeping.SimulateOvertimeRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.SimulateOvertimeRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (value.Timesheet?.BillingAddress?.Recipient ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _timekeepingOvertimeCalculationService.SimulateOvertimeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.ScoreMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Timesheet?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _metricsMetricSeriesValidationService.ScoreMetricSeriesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.SignalTimesheetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.MetricSeries?.Address?.Recipient ?? ""),
                },
                TimesheetId = (request.TimesheetId ?? ""),
                Total = (request.Timesheet?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "SignalTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.StartTimesheetWorkflowResponse> StartTimesheetWorkflow(ApiTimekeeping.StartTimesheetWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartTimesheetWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.StartTimesheetWorkflowToPreviewTimesheet0();
        using var rpc0 = _timekeepingTimesheetValidationService.PreviewTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.CountClockEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.CountClockEventsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Timesheet?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Timesheet?.TimesheetId ?? ""),
                Region = (reply0.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await SendStartTimesheetWorkflow1Async(input1, token);
        var response = new ApiTimekeeping.StartTimesheetWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Timesheet?.Counterparty?.DisplayName ?? ""),
                },
                Status = (request.Timesheet?.Status ?? (ApiTimekeeping.TimesheetStatus)0),
                TimesheetId = (request.TimesheetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "StartTimesheetWorkflow", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiTimekeeping.CountClockEventsResponse> SendStartTimesheetWorkflow1Async(ApiTimekeeping.CountClockEventsRequest request, CancellationToken token)
    {
        return await _timesheetWorkflowEndpointSendStartTimesheetWorkflow1Port.SendAsync(request, token);
    }
}
