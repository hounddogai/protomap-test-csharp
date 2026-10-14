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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Timekeeping;

/// <summary>
/// Computes the figures of an overtime, such as totals, taxes, and estimates.
/// </summary>
public sealed class OvertimeCalculationEndpoint : ApiTimekeeping.OvertimeCalculationService.OvertimeCalculationServiceBase
{
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient _learningCourseEnrollmentCalculationService;
    private readonly ILogger<OvertimeCalculationEndpoint> _logger;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient _timekeepingClockEventCalculationService;
    private readonly ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient _timekeepingTimesheetValidationService;

    public OvertimeCalculationEndpoint(
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        CallInvoker invoker,
        ApiLearning.CourseEnrollmentCalculationService.CourseEnrollmentCalculationServiceClient learningCourseEnrollmentCalculationService,
        ILogger<OvertimeCalculationEndpoint> logger,
        RetryPolicy policy,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTimekeeping.ClockEventCalculationService.ClockEventCalculationServiceClient timekeepingClockEventCalculationService,
        ApiTimekeeping.TimesheetValidationService.TimesheetValidationServiceClient timekeepingTimesheetValidationService)
    {
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _invoker = invoker;
        _learningCourseEnrollmentCalculationService = learningCourseEnrollmentCalculationService;
        _logger = logger;
        _policy = policy;
        _repository = repository;
        _settings = settings;
        _timekeepingClockEventCalculationService = timekeepingClockEventCalculationService;
        _timekeepingTimesheetValidationService = timekeepingTimesheetValidationService;
    }
    /// <summary>
    /// Calculates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.CalculateOvertimeResponse> CalculateOvertime(ApiTimekeeping.CalculateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.PreviewTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Overtime?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Amount = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                    {
                        ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _timekeepingTimesheetValidationService.PreviewTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ScoreTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ScoreTimesheetRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Timesheet?.BillingAddress?.CountryCode ?? ""),
                },
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Overtime?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiTimekeeping.ScoreTimesheetRequest, ApiTimekeeping.ScoreTimesheetResponse>(
            MethodType.Unary, "acme.timekeeping.v1" + ".TimesheetValidationService", "ScoreTimesheet",
            Wire.Marshaller<ApiTimekeeping.ScoreTimesheetRequest>(), Wire.Marshaller<ApiTimekeeping.ScoreTimesheetResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.CalculateOvertimeResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Timesheet?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OvertimeId = (request.OvertimeId ?? ""),
                Priority = (request.Overtime?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "CalculateOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.CompareOvertimeOptionsResponse> CompareOvertimeOptions(ApiTimekeeping.CompareOvertimeOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareOvertimeOptions for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ValidateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Overtime?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiTimekeeping.ValidateTimesheetRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _timekeepingTimesheetValidationService.ValidateTimesheetAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.PreviewTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Timesheet?.BillingAddress?.PostalCode ?? ""),
                },
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Tax = new ApiTimekeeping.TimesheetLineTax
                    {
                        Amount = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                        {
                            ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _timekeepingTimesheetValidationService.PreviewTimesheetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.CompareOvertimeOptionsResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Timesheet?.Audit?.UpdatedBy ?? ""),
                },
                Description = (request.Overtime?.Description ?? ""),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "CompareOvertimeOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.EstimateOvertimeResponse> EstimateOvertime(ApiTimekeeping.EstimateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ExplainTimesheetDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Tax = new ApiTimekeeping.TimesheetLineTax
                    {
                        Amount = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                        {
                            ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _timekeepingTimesheetValidationService.ExplainTimesheetDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.EstimateOvertimeResponse
        {
            Breakdown = new ApiTimekeeping.EstimateOvertimeResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Timesheet?.Audit?.CreatedBy ?? ""),
                },
                OvertimeId = (request.OvertimeId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "EstimateOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.ExplainOvertimeCalculationResponse> ExplainOvertimeCalculation(ApiTimekeeping.ExplainOvertimeCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainOvertimeCalculation for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ExplainOvertimeCalculationToValidateTimesheet0();
        using var rpc0 = _timekeepingTimesheetValidationService.ValidateTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.QuoteClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.Overtime?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.QuoteClockEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Timesheet?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (reply0.Timesheet?.BillingAddress?.Recipient ?? ""),
                    Region = (reply0.Timesheet?.BillingAddress?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _timekeepingClockEventCalculationService.QuoteClockEventAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.SubscribeSignatureUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeSignatureUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc2 = _documentsSignatureNotificationService.SubscribeSignatureUpdatesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiTimekeeping.ExplainOvertimeCalculationResponse
        {
            Outcome = new ApiTimekeeping.ExplainOvertimeCalculationResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Owner = (request.Overtime?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Signature?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "ExplainOvertimeCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.GetOvertimeRatesResponse> GetOvertimeRates(ApiTimekeeping.GetOvertimeRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OvertimeId, "overtime_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetOvertimeRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "overtime", request.OvertimeId,
            "GetOvertimeRates", "accepted", token);
        var input0 = new ApiTimekeeping.ValidateTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.ValidateTimesheetRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Timesheet = new ApiTimekeeping.Timesheet
            {
                IssuedAt = (request.Context?.Deadline ?? new Timestamp()),
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiTimekeeping.ValidateTimesheetRequest>();
        using var rpc0 = _timekeepingTimesheetValidationService.ValidateTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.GetOvertimeRatesResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.QuoteOvertimeResponse> QuoteOvertime(ApiTimekeeping.QuoteOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.ScoreTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Overtime?.Audit?.CreatedBy ?? ""),
                },
                Lines = { new ApiTimekeeping.TimesheetLine
                {
                    Tax = new ApiTimekeeping.TimesheetLineTax
                    {
                        Amount = (request.Overtime ?? new ApiTimekeeping.Overtime()).ActionCase switch
                        {
                            ApiTimekeeping.Overtime.ActionOneofCase.Discount => (request.Overtime?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _timekeepingTimesheetValidationService.ScoreTimesheetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTimekeeping.ExplainTimesheetDecisionRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Timesheet = new ApiTimekeeping.Timesheet
        {
            BillingAddress = new ApiCommon.PostalAddress
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc1 = _timekeepingTimesheetValidationService.ExplainTimesheetDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTimekeeping.QuoteOvertimeResponse
        {
            Breakdown = new ApiTimekeeping.QuoteOvertimeResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
            },
            Overtime = new ApiTimekeeping.Overtime
            {
                Effective = (request.Overtime?.Effective ?? new ApiCommon.TimeWindow()),
                OvertimeId = (request.OvertimeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "QuoteOvertime", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.RecalculateOvertimesResponse> RecalculateOvertimes(ApiTimekeeping.RecalculateOvertimesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateOvertimes for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NotifySignatureWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifySignatureWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureNotificationService.NotifySignatureWatchersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTimekeeping.RecalculateOvertimesResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                OvertimeId = (request.OvertimeId ?? ""),
                Priority = (request.Overtime?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "RecalculateOvertimes", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates an overtime.
    /// </summary>
    public override async Task<ApiTimekeeping.SimulateOvertimeResponse> SimulateOvertime(ApiTimekeeping.SimulateOvertimeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateOvertime for {Tenant}", requestContext.TenantId);
        if (request.Overtime is { } record)
        {
            var snapshot = OvertimeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.NormalizeTimesheetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Timesheet = new ApiTimekeeping.Timesheet
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Overtime?.Owner?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _timekeepingTimesheetValidationService.NormalizeTimesheetAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLearning.CalculateCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Card = new ApiLearning.CourseEnrollmentCardFunding
                {
                    HolderName = (reply0.Timesheet ?? new ApiTimekeeping.Timesheet()).PaymentCase switch
                    {
                        ApiTimekeeping.Timesheet.SettlementOneofCase.Card => (reply0.Timesheet?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _learningCourseEnrollmentCalculationService.CalculateCourseEnrollmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTimekeeping.SimulateOvertimeResponse
        {
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.CourseEnrollment?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.Timesheet?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OvertimeId = (request.OvertimeId ?? ""),
                Status = (request.Overtime?.Status ?? (ApiTimekeeping.OvertimeStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "overtime",
            "audit", "SimulateOvertime", (request.Reason ?? ""), token);
        return response;
    }
}
