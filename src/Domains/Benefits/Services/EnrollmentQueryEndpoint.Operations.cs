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
using ApiAudit = Acme.Audit.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Benefits;

public sealed partial class EnrollmentQueryEndpoint
{
    /// <summary>
    /// Lists enrollments a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.BrowseEnrollmentsResponse> BrowseEnrollments(ApiBenefits.BrowseEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "BrowseEnrollments", "accepted", token);
        var input0 = new ApiMetrics.CheckMetricSeriesEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _metricsMetricSeriesValidationService.CheckMetricSeriesEligibilityAsync);
        var input1 = new ApiLabeling.SuggestLabelingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLabeling.SuggestLabelingTasksRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc1 = _sendBrowseEnrollments1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.BrowseEnrollmentsResponse
        {
            Enrollments = { new ApiBenefits.Enrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.MetricSeries?.Audit?.CreatedBy ?? ""),
                },
            } },
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply1.PageInfo?.NextPageToken ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "BrowseEnrollments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts enrollments by status.
    /// </summary>
    public override async Task<ApiBenefits.CountEnrollmentsResponse> CountEnrollments(ApiBenefits.CountEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "CountEnrollments", "accepted", token);
        var response = new ApiBenefits.CountEnrollmentsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "CountEnrollments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching enrollment.
    /// </summary>
    public override async Task ExportEnrollments(ApiBenefits.ExportEnrollmentsRequest request, IServerStreamWriter<ApiBenefits.ExportEnrollmentsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "ExportEnrollments", "accepted", token);
        var input0 = new ApiBenefits.RecalculateLifeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RecalculateLifeEventsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Attributes = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiBenefits.LifeEventMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.RecalculateLifeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.ExportEnrollmentsResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "ExportEnrollments", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists enrollments a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.FindEnrollmentsResponse> FindEnrollments(ApiBenefits.FindEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "FindEnrollments", "accepted", token);
        var input0 = new ApiHelpdesk.BrowseTicketsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.BrowseTicketsRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageSize = (request.Page?.PageSize ?? 0),
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.BrowseTicketsRequest, ApiHelpdesk.BrowseTicketsResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".TicketQueryService", "BrowseTickets",
            Wire.Marshaller<ApiHelpdesk.BrowseTicketsRequest>(), Wire.Marshaller<ApiHelpdesk.BrowseTicketsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBenefits.RecalculateLifeEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.RecalculateLifeEventsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiBenefits.LifeEventMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _benefitsLifeEventCalculationService.RecalculateLifeEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.FindEnrollmentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "FindEnrollments", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.LookupEnrollmentResponse> LookupEnrollment(ApiBenefits.LookupEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EnrollmentId, "enrollment_id");
        _logger.LogInformation("Handling LookupEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", request.EnrollmentId,
            "LookupEnrollment", "accepted", token);
        var response = new ApiBenefits.LookupEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.PeekEnrollmentResponse> PeekEnrollment(ApiBenefits.PeekEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EnrollmentId, "enrollment_id");
        _logger.LogInformation("Handling PeekEnrollment for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", request.EnrollmentId,
            "PeekEnrollment", "accepted", token);
        var profile0 = new MappingProfile<ApiBenefits.PeekEnrollmentRequest, ApiBenefits.FindBenefitPlansRequest>(value => new ApiBenefits.FindBenefitPlansRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBenefits.FindBenefitPlansRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                Labels = { (value.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (value.EnrollmentId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _benefitsBenefitPlanQueryService.FindBenefitPlansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentTemplateVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
            {
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiBenefits.PeekEnrollmentResponse
        {
            Enrollment = new ApiBenefits.Enrollment
            {
                EnrollmentId = (request.EnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several enrollments at once.
    /// </summary>
    public override async Task<ApiBenefits.ResolveEnrollmentsResponse> ResolveEnrollments(ApiBenefits.ResolveEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "ResolveEnrollments", "accepted", token);
        var input0 = new ApiAudit.WatchAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudit.WatchAuditEventsRequest.Types.Filter
            {
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Context?.Actor?.UserId ?? ""),
            },
            Since = (request.Context?.Deadline ?? new Timestamp()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _auditAuditEventService.WatchAuditEvents(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAuditEvents frame {Type}", change0.GetType().Name);
        }
        var response = new ApiBenefits.ResolveEnrollmentsResponse();
        return response;
    }
    /// <summary>
    /// Lists enrollments a page at a time.
    /// </summary>
    public override async Task<ApiBenefits.SuggestEnrollmentsResponse> SuggestEnrollments(ApiBenefits.SuggestEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", "batch",
            "SuggestEnrollments", "accepted", token);
        var input0 = new ApiBenefits.CompareLifeEventOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.CompareLifeEventOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _benefitsLifeEventCalculationService.CompareLifeEventOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiBenefits.SuggestEnrollmentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "SuggestEnrollments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes an enrollment.
    /// </summary>
    public override async Task<ApiBenefits.SummarizeEnrollmentsResponse> SummarizeEnrollments(ApiBenefits.SummarizeEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "enrollment", request.EnrollmentId,
            "SummarizeEnrollments", "accepted", token);
        var input0 = new ApiBenefits.RecalculateLifeEventsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _benefitsLifeEventCalculationService.RecalculateLifeEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CalculateEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.LifeEvent?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiHelpdesk.CalculateEscalationRequest>();
        using var rpc1 = _helpdeskEscalationCalculationService.CalculateEscalationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMetrics.ValidateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.ValidateMetricSeriesRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Escalation?.Assignee?.Email ?? ""),
                    Name = (reply1.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Escalation?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Escalation?.Assignee?.UserId ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiMetrics.ValidateMetricSeriesRequest>(json2);
        using var rpc2 = _metricsMetricSeriesValidationService.ValidateMetricSeriesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiBenefits.ExplainLifeEventDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.ExplainLifeEventDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.LifeEvent?.Subject?.Email ?? ""),
                    Name = (reply1.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            LifeEvent = new ApiBenefits.LifeEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply1.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply1.Escalation?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _benefitsLifeEventValidationService.ExplainLifeEventDecisionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiBenefits.SummarizeEnrollmentsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.LifeEvent?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.EnrollmentId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "enrollment",
            "audit", "SummarizeEnrollments", (request.EnrollmentId ?? ""), token);
        return response;
    }
}
