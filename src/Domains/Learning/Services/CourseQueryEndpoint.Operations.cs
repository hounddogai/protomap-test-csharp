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
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CourseQueryEndpoint
{
    /// <summary>
    /// Lists courses a page at a time.
    /// </summary>
    public override async Task<ApiLearning.BrowseCoursesResponse> BrowseCourses(ApiLearning.BrowseCoursesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseCourses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", "batch",
            "BrowseCourses", "accepted", token);
        var response = new ApiLearning.BrowseCoursesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "BrowseCourses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts courses by status.
    /// </summary>
    public override async Task<ApiLearning.CountCoursesResponse> CountCourses(ApiLearning.CountCoursesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountCourses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", "batch",
            "CountCourses", "accepted", token);
        var input0 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await SendCountCourses0Async(input0, token);
        var response = new ApiLearning.CountCoursesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "CountCourses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching course.
    /// </summary>
    public override async Task ExportCourses(ApiLearning.ExportCoursesRequest request, IServerStreamWriter<ApiLearning.ExportCoursesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportCourses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", "batch",
            "ExportCourses", "accepted", token);
        var input0 = new ApiItsm.ExportIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ExportIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var stream0 = _itsmIncidentQueryService.ExportIncidents(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportIncidents frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.SuggestDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiDocuments.SuggestDocumentFoldersRequest>();
        using var rpc1 = _documentsDocumentFolderQueryService.SuggestDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ExportCoursesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "ExportCourses", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists courses a page at a time.
    /// </summary>
    public override async Task<ApiLearning.FindCoursesResponse> FindCourses(ApiLearning.FindCoursesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindCourses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", "batch",
            "FindCourses", "accepted", token);
        var response = new ApiLearning.FindCoursesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "FindCourses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course.
    /// </summary>
    public override async Task<ApiLearning.LookupCourseResponse> LookupCourse(ApiLearning.LookupCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseId, "course_id");
        _logger.LogInformation("Handling LookupCourse for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "LookupCourse", "accepted", token);
        var response = new ApiLearning.LookupCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a course.
    /// </summary>
    public override async Task<ApiLearning.PeekCourseResponse> PeekCourse(ApiLearning.PeekCourseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseId, "course_id");
        _logger.LogInformation("Handling PeekCourse for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "PeekCourse", "accepted", token);
        var response = new ApiLearning.PeekCourseResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists courses a page at a time.
    /// </summary>
    public override async Task<ApiLearning.SuggestCoursesResponse> SuggestCourses(ApiLearning.SuggestCoursesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestCourses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", "batch",
            "SuggestCourses", "accepted", token);
        var input0 = new ApiTravel.CompareTripOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CompareTripOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiTravel.CompareTripOptionsRequest>(json0);
        using var rpc0 = _travelTripCalculationService.CompareTripOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.SuggestCoursesResponse
        {
            Courses = { new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.Trip?.Audit?.UpdatedBy ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "SuggestCourses", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a course.
    /// </summary>
    public override async Task<ApiLearning.SummarizeCoursesResponse> SummarizeCourses(ApiLearning.SummarizeCoursesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeCourses for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "SummarizeCourses", "accepted", token);
        var input0 = new ApiLearning.ExplainCourseDecisionRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Course = new ApiLearning.Course
        {
            Owner = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        input0.Details = new ApiLearning.ExplainCourseDecisionRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc0 = _learningCourseValidationService.ExplainCourseDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.EstimateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _learningCertificationCalculationService.EstimateCertificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.SummarizeCoursesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "SummarizeCourses", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.ValidateDocumentResponse> SendCountCourses0Async(ApiDocuments.ValidateDocumentRequest request, CancellationToken token)
    {
        return await _courseQueryEndpointSendCountCourses0Port.SendAsync(request, token);
    }
}
