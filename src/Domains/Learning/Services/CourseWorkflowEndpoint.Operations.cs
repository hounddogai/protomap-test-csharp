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
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CourseWorkflowEndpoint
{
    /// <summary>
    /// Advances a course.
    /// </summary>
    public override async Task<ApiLearning.AdvanceCourseWorkflowResponse> AdvanceCourseWorkflow(ApiLearning.AdvanceCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceCourseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAudit.ScoreAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.ScoreAuditEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _auditAuditEventValidationService.ScoreAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SimulateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Course?.Audit?.Etag ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _recruitingCandidateCalculationService.SimulateCandidateAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.AdvanceCourseWorkflowResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Candidate?.RelationshipManager?.UserId ?? ""),
                },
                Status = (request.Course?.Status ?? (ApiLearning.CourseStatus)0),
            },
            Outcome = new ApiLearning.AdvanceCourseWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "AdvanceCourseWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a course.
    /// </summary>
    public override async Task<ApiLearning.CancelCourseWorkflowResponse> CancelCourseWorkflow(ApiLearning.CancelCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelCourseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.EstimateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _recruitingCandidateCalculationService.EstimateCandidateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.CancelCourseWorkflowResponse
        {
            Course = new ApiLearning.Course
            {
                CourseId = (request.CourseId ?? ""),
                Name = (request.Course?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "CancelCourseWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course.
    /// </summary>
    public override async Task<ApiLearning.InspectCourseWorkflowResponse> InspectCourseWorkflow(ApiLearning.InspectCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseId, "course_id");
        _logger.LogInformation("Handling InspectCourseWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", request.CourseId,
            "InspectCourseWorkflow", "accepted", token);
        var response = new ApiLearning.InspectCourseWorkflowResponse
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
    public override async Task<ApiLearning.ListCourseWorkflowsResponse> ListCourseWorkflows(ApiLearning.ListCourseWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListCourseWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course", "batch",
            "ListCourseWorkflows", "accepted", token);
        var input0 = new ApiLearning.BrowseLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.BrowseLearningPathsRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        using var rpc0 = _learningLearningPathQueryService.BrowseLearningPathsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.ListCourseWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "ListCourseWorkflows", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a course.
    /// </summary>
    public override async Task<ApiLearning.PauseCourseWorkflowResponse> PauseCourseWorkflow(ApiLearning.PauseCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseCourseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SimulateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SimulateSignatureRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    Unit = (request.Course?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsSignatureCalculationService.SimulateSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.CheckLearningPathEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                            {
                                ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                                _ => new Google.Protobuf.Collections.RepeatedField<string>()
                            } },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathValidationService.CheckLearningPathEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.RejectLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply1.LearningPath?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply1.LearningPath?.Location?.Locality ?? ""),
                        },
                    } },
                } },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _learningLearningPathApprovalService.RejectLearningPathAsync);
        var response = new ApiLearning.PauseCourseWorkflowResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.LearningPath?.Audit?.Etag ?? ""),
                },
                CourseId = (request.CourseId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.LearningPath?.Manager?.Email ?? ""),
                },
                Specs = { (request.Course?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiLearning.PauseCourseWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "PauseCourseWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a course.
    /// </summary>
    public override async Task<ApiLearning.ResumeCourseWorkflowResponse> ResumeCourseWorkflow(ApiLearning.ResumeCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeCourseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.DelegateCourseApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _learningCourseApprovalService.DelegateCourseApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CountVisitorPassTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountVisitorPassTotalsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Course?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Course?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _workplacesVisitorPassReportingService.CountVisitorPassTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ResumeCourseWorkflowResponse
        {
            Course = new ApiLearning.Course
            {
                Capacity = (request.Course?.Capacity ?? 0L),
                Components = { { "primary", new ApiLearning.CourseComponent
                {
                    Quantity = (reply0.Course?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } } },
                CourseId = (request.CourseId ?? ""),
            },
            Outcome = new ApiLearning.ResumeCourseWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "ResumeCourseWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a course.
    /// </summary>
    public override async Task<ApiLearning.RetryCourseWorkflowResponse> RetryCourseWorkflow(ApiLearning.RetryCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryCourseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ListPendingGoalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.ListPendingGoalsRequest.Types.Filter
            {
                CreatedAfter = (request.Course?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Course?.CourseId ?? ""),
                Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _performanceGoalApprovalService.ListPendingGoalsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.CommentCourseApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Dimensions = new ApiLearning.CourseDimensions
                {
                    Length = (request.Course?.Dimensions?.Length ?? 0d),
                    Weight = (request.Course?.Dimensions?.Weight ?? 0d),
                },
            },
            Details = new ApiLearning.CommentCourseApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Course?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLearning.CommentCourseApprovalRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _learningCourseApprovalService.CommentCourseApprovalAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ListLicenseAssignmentReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListLicenseAssignmentReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                {
                    ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentReportingService.ListLicenseAssignmentReportsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.DelegateLearningPathApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Details?.Notify?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        input3 = PropertyMap.Copy(input3);
        using var rpc3 = _learningLearningPathApprovalService.DelegateLearningPathApprovalAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLearning.RetryCourseWorkflowResponse
        {
            Course = new ApiLearning.Course
            {
                Components = { { "primary", new ApiLearning.CourseComponent
                {
                    ComponentId = (reply1.Course?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                } } },
                CourseId = (request.CourseId ?? ""),
                Owner = (request.Course?.Owner ?? new ApiIdentity.UserRef()),
                Tags = { (reply1.Course?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "RetryCourseWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a course.
    /// </summary>
    public override async Task<ApiLearning.StartCourseWorkflowResponse> StartCourseWorkflow(ApiLearning.StartCourseWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartCourseWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Course is { } record)
        {
            var snapshot = CourseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.PreviewCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Course?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Course?.Owner?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (request.Course?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _recruitingCandidateValidationService.PreviewCandidateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ValidateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Course?.Audit?.UpdatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Candidate?.RelationshipManager?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    UserId = (reply0.Candidate?.RelationshipManager?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await SendStartCourseWorkflow1Async(input1, token);
        var input2 = new ApiRecruiting.ExplainCandidateDecisionRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                NationalId = new ApiRecruiting.CandidateNationalId
                {
                    Number = (reply1.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.NationalId => (reply1.Candidate?.NationalId?.Number ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _recruitingCandidateValidationService.ExplainCandidateDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.ExplainCandidateDecisionRequest>("acme.learning.v1.CourseWorkflowService/StartCourseWorkflow/2", input2.Clone()), token);
        var response = new ApiLearning.StartCourseWorkflowResponse
        {
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Candidate?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Code = (request.Course?.Code ?? ""),
                CourseId = (request.CourseId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course",
            "audit", "StartCourseWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.ValidateCandidateResponse> SendStartCourseWorkflow1Async(ApiRecruiting.ValidateCandidateRequest request, CancellationToken token)
    {
        return await _courseWorkflowEndpointSendStartCourseWorkflow1Port.SendAsync(request, token);
    }
}
