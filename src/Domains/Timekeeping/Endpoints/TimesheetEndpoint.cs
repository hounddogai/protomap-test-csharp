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
using ApiMetrics = Acme.Metrics.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Manages timesheets: creates, reads, changes, and lists them.
/// </summary>
public sealed class TimesheetEndpoint : ApiTimekeeping.TimesheetService.TimesheetServiceBase
{
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<TimesheetEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient _schedulingSwapRequestValidationService;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly Func<ApiTimekeeping.ExplainOvertimeCalculationRequest, CallOptions, AsyncUnaryCall<ApiTimekeeping.ExplainOvertimeCalculationResponse>> _sendDeleteTimesheet0;
    private readonly ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient _timekeepingClockEventValidationService;
    private readonly ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient _timekeepingOvertimeCalculationService;
    private readonly ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient _timekeepingTimesheetQueryService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;

    public TimesheetEndpoint(
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        CallInvoker invoker,
        ILogger<TimesheetEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.SwapRequestValidationService.SwapRequestValidationServiceClient schedulingSwapRequestValidationService,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        ApiTimekeeping.ClockEventValidationService.ClockEventValidationServiceClient timekeepingClockEventValidationService,
        ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceClient timekeepingOvertimeCalculationService,
        ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient timekeepingTimesheetQueryService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService)
    {
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _invoker = invoker;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _schedulingSwapRequestValidationService = schedulingSwapRequestValidationService;
        _searchSearchIndexService = searchSearchIndexService;
        _timekeepingClockEventValidationService = timekeepingClockEventValidationService;
        _timekeepingOvertimeCalculationService = timekeepingOvertimeCalculationService;
        _timekeepingTimesheetQueryService = timekeepingTimesheetQueryService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
        _sendDeleteTimesheet0 = _timekeepingOvertimeCalculationService.ExplainOvertimeCalculationAsync;
    }
    /// <summary>
    /// Takes a stream of timesheets and answers once at the end.
    /// </summary>
    public override async Task<ApiTimekeeping.BatchCreateTimesheetsResponse> BatchCreateTimesheets(IAsyncStreamReader<ApiTimekeeping.BatchCreateTimesheetsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiTimekeeping.BatchCreateTimesheetsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateTimesheets for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiPerformance.SimulateFeedbackRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiPerformance.SimulateFeedbackRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        PostalCode = request.ChunkCase switch
                        {
                            ApiTimekeeping.BatchCreateTimesheetsRequest.ChunkOneofCase.Record => (request.Record?.BillingAddress?.PostalCode ?? ""),
                            _ => ""
                        },
                        Recipient = request.ChunkCase switch
                        {
                            ApiTimekeeping.BatchCreateTimesheetsRequest.ChunkOneofCase.Record => (request.Record?.BillingAddress?.Recipient ?? ""),
                            _ => ""
                        },
                    },
                },
                Feedback = new ApiPerformance.Feedback
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                },
            };
            using var rpc0 = _performanceFeedbackCalculationService.SimulateFeedbackAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiTimekeeping.ValidateClockEventRequest
            {
                ClockEvent = new ApiTimekeeping.ClockEvent
                {
                    CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiTimekeeping.TimesheetRef
                    {
                        DisplayName = (reply0.Feedback?.Subject?.DisplayName ?? ""),
                        Number = request.ChunkCase switch
                        {
                            ApiTimekeeping.BatchCreateTimesheetsRequest.ChunkOneofCase.Record => (request.Record?.Number ?? ""),
                            _ => ""
                        },
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiTimekeeping.ValidateClockEventRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Feedback?.Subject?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _timekeepingClockEventValidationService.ValidateClockEventAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var response = new ApiTimekeeping.BatchCreateTimesheetsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                    ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Status = (reply1.Result?.Status ?? ""),
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.VoucherCode ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "timesheet",
                "audit", "BatchCreateTimesheets", (request.Record?.VoucherCode ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several timesheets at once.
    /// </summary>
    public override async Task<ApiTimekeeping.BatchGetTimesheetsResponse> BatchGetTimesheets(ApiTimekeeping.BatchGetTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BatchGetTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "BatchGetTimesheets", "accepted", token);
        var input0 = new ApiTimekeeping.ValidateClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ValidateClockEventRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _timekeepingClockEventValidationService.ValidateClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.BatchGetTimesheetsResponse
        {
            Timesheets = { new ApiTimekeeping.Timesheet
            {
                Number = (reply0.ClockEvent?.Subject?.Number ?? ""),
            } },
        };
        return response;
    }
    /// <summary>
    /// Creates a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.CloneTimesheetResponse> CloneTimesheet(ApiTimekeeping.CloneTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.SimulateOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Timesheet?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingOvertimeCalculationService.SimulateOvertimeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.CloneMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Overtime?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesService.CloneMetricSeriesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiSearch.BatchGetSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _searchSearchIndexService.BatchGetSearchIndexesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTimekeeping.CloneTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Overtime?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = (request.Timesheet?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Description = (reply1.MetricSeries?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
                Number = (request.Timesheet?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "CloneTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.CreateTimesheetResponse> CreateTimesheet(ApiTimekeeping.CreateTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ExplainOvertimeCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ExplainOvertimeCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Timesheet?.Counterparty?.Email ?? ""),
                },
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Timesheet?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Timesheet?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _timekeepingOvertimeCalculationService.ExplainOvertimeCalculationAsync);
        var response = new ApiTimekeeping.CreateTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                DueAt = (request.Timesheet?.DueAt ?? new Timestamp()),
                Total = (request.Timesheet?.Total ?? new ApiCommon.Money()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "CreateTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.DeleteTimesheetResponse> DeleteTimesheet(ApiTimekeeping.DeleteTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TimesheetId, "timesheet_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteTimesheet for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "DeleteTimesheet", "accepted", token);
        var input0 = new ApiTimekeeping.ExplainOvertimeCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Block = new ApiTimekeeping.OvertimeBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _sendDeleteTimesheet0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.DeleteTimesheetResponse
        {
            EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TimesheetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "DeleteTimesheet", (request.TimesheetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.GetTimesheetResponse> GetTimesheet(ApiTimekeeping.GetTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TimesheetId, "timesheet_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTimesheet for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", request.TimesheetId,
            "GetTimesheet", "accepted", token);
        var response = new ApiTimekeeping.GetTimesheetResponse
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
    public override async Task<ApiTimekeeping.SearchTimesheetsResponse> SearchTimesheets(ApiTimekeeping.SearchTimesheetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "SearchTimesheets", "accepted", token);
        var response = new ApiTimekeeping.SearchTimesheetsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "SearchTimesheets", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a timesheet.
    /// </summary>
    public override async Task<ApiTimekeeping.UpdateTimesheetResponse> UpdateTimesheet(ApiTimekeeping.UpdateTimesheetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateTimesheet for {Tenant}", requestContext.TenantId);
        if (request.Timesheet is { } record)
        {
            var snapshot = TimesheetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.FindTimesheetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.FindTimesheetsRequest.Types.Filter
            {
                Region = (request.Timesheet?.BillingAddress?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiTimekeeping.TimesheetQueryService.TimesheetQueryServiceClient>("acme.timekeeping.v1.TimesheetQueryService");
        using var rpc0 = named0.FindTimesheetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.UpdateTimesheetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Metadata = { (request.Timesheet?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (request.Timesheet?.Status ?? (ApiTimekeeping.TimesheetStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Timesheet?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "UpdateTimesheet", (request.Timesheet?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of timesheets as they happen.
    /// </summary>
    public override async Task WatchTimesheets(ApiTimekeeping.WatchTimesheetsRequest request, IServerStreamWriter<ApiTimekeeping.WatchTimesheetsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchTimesheets for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "timesheet", "batch",
            "WatchTimesheets", "accepted", token);
        var input0 = new ApiScheduling.NormalizeSwapRequestRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiScheduling.NormalizeSwapRequestRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
            SwapRequest = new ApiScheduling.SwapRequest
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiScheduling.NormalizeSwapRequestRequest, ApiScheduling.NormalizeSwapRequestResponse>(
            MethodType.Unary, "acme.scheduling.v1" + ".SwapRequestValidationService", "NormalizeSwapRequest",
            Wire.Marshaller<ApiScheduling.NormalizeSwapRequestRequest>(), Wire.Marshaller<ApiScheduling.NormalizeSwapRequestResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.UnsubscribeDocumentUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SwapRequest?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentNotificationService.UnsubscribeDocumentUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiTimekeeping.WatchTimesheetsRequest, ApiTimekeeping.SummarizeTimesheetsRequest>(value => new ApiTimekeeping.SummarizeTimesheetsRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.SummarizeTimesheetsRequest.Types.Details
            {
                Filters = { (value.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _timekeepingTimesheetQueryService.SummarizeTimesheetsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTimekeeping.ScoreTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ScoreTimesheetRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Document?.BillingAddress?.Locality ?? ""),
                },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _timekeepingTimesheetValidationService.ScoreTimesheetAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTimekeeping.WatchTimesheetsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.SwapRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                IssuedAt = (reply3.Timesheet?.IssuedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.Query ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "timesheet",
            "audit", "WatchTimesheets", (request.Filter?.Query ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
