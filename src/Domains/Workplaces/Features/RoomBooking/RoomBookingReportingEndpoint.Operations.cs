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
using ApiCustomers = Acme.Customers.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class RoomBookingReportingEndpoint
{
    /// <summary>
    /// Counts room bookings by status.
    /// </summary>
    public override async Task<ApiWorkplaces.CountRoomBookingTotalsResponse> CountRoomBookingTotals(ApiWorkplaces.CountRoomBookingTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountRoomBookingTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", "batch",
            "CountRoomBookingTotals", "accepted", token);
        var input0 = new ApiWorkplaces.ExplainRoomBookingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiWorkplaces.ExplainRoomBookingCalculationRequest>();
        using var rpc0 = _workplacesRoomBookingCalculationService.ExplainRoomBookingCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                Region = (reply0.RoomBooking?.Location?.Region ?? ""),
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployees.BrowseOrgUnitsRequest>(json1);
        using var rpc1 = _employeesOrgUnitQueryService.BrowseOrgUnitsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.CountRoomBookingTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "CountRoomBookingTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.FetchRoomBookingReportResponse> FetchRoomBookingReport(ApiWorkplaces.FetchRoomBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RoomBookingId, "room_booking_id");
        _logger.LogInformation("Handling FetchRoomBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "FetchRoomBookingReport", "accepted", token);
        var response = new ApiWorkplaces.FetchRoomBookingReportResponse
        {
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.GenerateRoomBookingReportResponse> GenerateRoomBookingReport(ApiWorkplaces.GenerateRoomBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateRoomBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "GenerateRoomBookingReport", "accepted", token);
        var input0 = new ApiItsm.ShareChangeRequestReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ShareChangeRequestReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _itsmChangeRequestReportingService.ShareChangeRequestReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.GenerateRoomBookingReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "GenerateRoomBookingReport", (request.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists room bookings a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListRoomBookingReportsResponse> ListRoomBookingReports(ApiWorkplaces.ListRoomBookingReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListRoomBookingReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", "batch",
            "ListRoomBookingReports", "accepted", token);
        var response = new ApiWorkplaces.ListRoomBookingReportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ListRoomBookingReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ScheduleRoomBookingReportResponse> ScheduleRoomBookingReport(ApiWorkplaces.ScheduleRoomBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleRoomBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "ScheduleRoomBookingReport", "accepted", token);
        var input0 = new ApiLicensing.EscalateSoftwareLicenseRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EscalateSoftwareLicenseRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseApprovalService.EscalateSoftwareLicenseAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.SummarizeDesksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.SummarizeDesksRequest.Types.Details
            {
                Filters = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
                RecipientEmails = { (request.Details?.RecipientEmails ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _workplacesDeskQueryService.SummarizeDesksAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiWorkplaces.ScheduleRoomBookingReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            ReportId = (reply1.ReportId ?? ""),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RoomBookingId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ScheduleRoomBookingReport", (request.RoomBookingId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a room booking.
    /// </summary>
    public override async Task<ApiWorkplaces.ShareRoomBookingReportResponse> ShareRoomBookingReport(ApiWorkplaces.ShareRoomBookingReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareRoomBookingReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", request.RoomBookingId,
            "ShareRoomBookingReport", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.RoomBookingId ?? ""),
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiWorkplaces.ExplainRoomBookingDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Participants = { new ApiWorkplaces.RoomBookingParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Details?.Recipient?.Phone ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _workplacesRoomBookingValidationService.ExplainRoomBookingDecisionAsync);
        var input2 = new ApiLedger.CreateLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.RoomBooking?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _sendShareRoomBookingReport2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ShareRoomBookingReportResponse
        {
            Outcome = new ApiWorkplaces.ShareRoomBookingReportResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = (reply1.RoomBooking?.Location ?? new ApiCommon.PostalAddress()),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                RoomBookingId = (request.RoomBookingId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "ShareRoomBookingReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of room bookings as they happen.
    /// </summary>
    public override async Task StreamRoomBookingMetrics(ApiWorkplaces.StreamRoomBookingMetricsRequest request, IServerStreamWriter<ApiWorkplaces.StreamRoomBookingMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamRoomBookingMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "room_booking", "batch",
            "StreamRoomBookingMetrics", "accepted", token);
        var input0 = new ApiCustomers.SearchCustomersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCustomers.SearchCustomersRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiCustomers.CustomerService.CustomerServiceClient>("acme.customers.v1.CustomerService");
        using var rpc0 = named0.SearchCustomersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCustomers.SearchCustomersRequest>("acme.workplaces.v1.RoomBookingReportingService/StreamRoomBookingMetrics/0", input0.Clone()), token);
        var input1 = new ApiWorkplaces.ResolveBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor1 = new Method<ApiWorkplaces.ResolveBuildingsRequest, ApiWorkplaces.ResolveBuildingsResponse>(
            MethodType.Unary, "acme.workplaces.v1" + ".BuildingQueryService", "ResolveBuildings",
            Wire.Marshaller<ApiWorkplaces.ResolveBuildingsRequest>(), Wire.Marshaller<ApiWorkplaces.ResolveBuildingsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.BatchCreateEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Record = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        using var stream2 = _employeesEmployeeService.BatchCreateEmployees(cancellationToken: token);
        await stream2.RequestStream.WriteAsync(input2, token);
        await stream2.RequestStream.CompleteAsync();
        var reply2 = await stream2.ResponseAsync;
        var profile3 = new MappingProfile<ApiWorkplaces.StreamRoomBookingMetricsRequest, ApiMetrics.CheckMetricSeriesEligibilityRequest>(value => new ApiMetrics.CheckMetricSeriesEligibilityRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Subscriber?.Email ?? ""),
                },
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _metricsMetricSeriesValidationService.CheckMetricSeriesEligibilityAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.StreamRoomBookingMetricsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            RoomBooking = new ApiWorkplaces.RoomBooking
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply3.MetricSeries?.Address?.CountryCode ?? ""),
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.MetricSeries?.Owner?.DisplayName ?? ""),
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.Phone ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "room_booking",
            "audit", "StreamRoomBookingMetrics", (request.Subscriber?.Phone ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
