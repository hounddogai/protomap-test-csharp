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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CourseEnrollmentApprovalEndpoint
{
    /// <summary>
    /// Approves a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ApproveCourseEnrollmentResponse> ApproveCourseEnrollment(ApiLearning.ApproveCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLearning.RecalculateCourseEnrollmentsResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiLearning.RecalculateCourseEnrollmentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                CourseEnrollment = new ApiLearning.CourseEnrollment
                {
                    Holder = new ApiRecruiting.CandidateRef
                    {
                        CandidateId = (request.CourseEnrollment?.Holder?.CandidateId ?? ""),
                    },
                    Limits = { new ApiLearning.CourseEnrollmentLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc0 = _learningCourseEnrollmentCalculationService.RecalculateCourseEnrollmentsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var response = new ApiLearning.ApproveCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                OpenedAt = (request.CourseEnrollment?.OpenedAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ApproveCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.CommentCourseEnrollmentApprovalResponse> CommentCourseEnrollmentApproval(ApiLearning.CommentCourseEnrollmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentCourseEnrollmentApproval for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.LintCandidatesRequest();
        input0.Candidate = new ApiRecruiting.Candidate
        {
            Contact = new ApiCommon.ContactInfo
            {
                PreferredChannel = (request.CourseEnrollment?.Contact?.PreferredChannel ?? ""),
            },
            RelationshipManager = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                Email = (request.Details?.Approver?.Email ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _recruitingCandidateValidationService.LintCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SuggestJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SuggestJobOpeningsRequest.Types.Filter
            {
                Region = (reply0.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _recruitingJobOpeningQueryService.SuggestJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.CommentCourseEnrollmentApprovalResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Holder = (request.CourseEnrollment?.Holder ?? new ApiRecruiting.CandidateRef()),
            },
            Outcome = new ApiLearning.CommentCourseEnrollmentApprovalResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "CommentCourseEnrollmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.DelegateCourseEnrollmentApprovalResponse> DelegateCourseEnrollmentApproval(ApiLearning.DelegateCourseEnrollmentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateCourseEnrollmentApproval for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainCandidateCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.NormalizeInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.NormalizeInterviewRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Candidate?.Contact?.Phone ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Candidate?.RelationshipManager?.Email ?? ""),
                    },
                } },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _recruitingInterviewValidationService.NormalizeInterviewAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var reply2 = new ApiLearning.NormalizeCertificationResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()))
        {
            var input2 = new ApiLearning.NormalizeCertificationRequest
            {
                Certification = new ApiLearning.Certification
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.Candidate?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        Region = (reply1.Interview?.Location?.Region ?? ""),
                    },
                    Lines = { new ApiLearning.CertificationLine
                    {
                        Tax = new ApiLearning.CertificationLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLearning.NormalizeCertificationRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                    },
                },
            };
            using var rpc2 = _learningCertificationValidationService.NormalizeCertificationAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiLearning.DelegateCourseEnrollmentApprovalResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Holder = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply1.Interview?.Organizer?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiLearning.DelegateCourseEnrollmentApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "DelegateCourseEnrollmentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists course enrollments a page at a time.
    /// </summary>
    public override async Task<ApiLearning.ListPendingCourseEnrollmentsResponse> ListPendingCourseEnrollments(ApiLearning.ListPendingCourseEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingCourseEnrollments for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", "batch",
            "ListPendingCourseEnrollments", "accepted", token);
        var input0 = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendListPendingCourseEnrollments0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.ListPendingCourseEnrollmentsResponse
        {
            CourseEnrollments = { new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.LicenseAssignment?.Audit?.Etag ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ListPendingCourseEnrollments", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.RecallCourseEnrollmentResponse> RecallCourseEnrollment(ApiLearning.RecallCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()).Select(async item =>
        {
            var input0 = new ApiSearch.LintSearchIndexesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                SearchIndex = new ApiSearch.SearchIndex
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (request.CourseEnrollment?.Audit?.UpdatedBy ?? ""),
                    },
                    Name = (item.Name ?? ""),
                    Owner = new ApiIdentity.UserRef
                    {
                        UserId = (request.Details?.Approver?.UserId ?? ""),
                    },
                },
            };
            using var rpc0 = _searchSearchIndexValidationService.LintSearchIndexesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiSearch.LintSearchIndexesResponse();
        var input1 = new ApiLearning.EstimateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (reply0.SearchIndex?.Audit?.Etag ?? ""),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.CourseEnrollment?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply0.SearchIndex ?? new ApiSearch.SearchIndex()).LocatorCase switch
                            {
                                ApiSearch.SearchIndex.LocatorOneofCase.Address => (reply0.SearchIndex?.Address?.PostalCode ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Details?.Approver?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _learningLearningPathCalculationService.EstimateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.SimulateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.LearningPath?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.SearchIndex?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.SearchIndex?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
        var command2 = new DispatchCommand<ApiRecruiting.SimulateInterviewRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _recruitingInterviewCalculationService.SimulateInterviewAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.RecallCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.LearningPath?.Audit?.UpdatedBy ?? ""),
                },
                Contact = (request.CourseEnrollment?.Contact ?? new ApiCommon.ContactInfo()),
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Outcome = new ApiLearning.RecallCourseEnrollmentResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "RecallCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.RejectCourseEnrollmentResponse> RejectCourseEnrollment(ApiLearning.RejectCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLearning.RejectCourseEnrollmentRequest, ApiCorporateevents.ResumeCorporateEventWorkflowRequest>(value => new ApiCorporateevents.ResumeCorporateEventWorkflowRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (value.CourseEnrollment?.Holder?.Email ?? ""),
                    },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsCorporateEventWorkflowService.ResumeCorporateEventWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ListPendingLicenseAssignmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListPendingLicenseAssignmentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _licensingLicenseAssignmentApprovalService.ListPendingLicenseAssignmentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.RejectCourseEnrollmentToCommentLearningPathApproval2();
        using var rpc2 = _learningLearningPathApprovalService.CommentLearningPathApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.LearningPath?.Location?.Recipient ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var reply3 = await SendRejectCourseEnrollment3Async(input3, token);
        var response = new ApiLearning.RejectCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                CurrencyCode = (request.CourseEnrollment?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "RejectCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.SubmitCourseEnrollmentResponse> SubmitCourseEnrollment(ApiLearning.SubmitCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CheckCandidateEligibilityRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.CourseEnrollment?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.CourseEnrollment?.Contact?.PreferredChannel ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingCandidateValidationService.CheckCandidateEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiRecruiting.LintJobOpeningsResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiRecruiting.LintJobOpeningsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiRecruiting.LintJobOpeningsRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Approver?.Email ?? ""),
                        Name = (request.CourseEnrollment?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                },
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Counterparty = new ApiRecruiting.InterviewRef
                    {
                        DisplayName = (request.CourseEnrollment?.Holder?.DisplayName ?? ""),
                    },
                    Lines = { new ApiRecruiting.JobOpeningLine
                    {
                        Tax = new ApiRecruiting.JobOpeningLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
            };
            using var rpc1 = _recruitingJobOpeningValidationService.LintJobOpeningsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiDocuments.BatchGetDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiDocuments.BatchGetDocumentsRequest>(json2);
        using var rpc2 = _documentsDocumentService.BatchGetDocumentsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiLicensing.SubmitLicenseAssignmentResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()))
        {
            var input3 = new ApiLicensing.SubmitLicenseAssignmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LicenseAssignment = new ApiLicensing.LicenseAssignment
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.Candidate?.Audit?.CreatedBy ?? ""),
                    },
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Locality = (reply0.Candidate?.PrimaryAddress?.Locality ?? ""),
                    },
                    Counterparty = new ApiLicensing.EntitlementRef
                    {
                        DisplayName = (reply1.JobOpening?.Counterparty?.DisplayName ?? ""),
                    },
                    Lines = { new ApiLicensing.LicenseAssignmentLine
                    {
                        Tax = new ApiLicensing.LicenseAssignmentLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                        },
                    } },
                },
            };
            using var rpc3 = _licensingLicenseAssignmentApprovalService.SubmitLicenseAssignmentAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiLearning.SubmitCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Candidate?.Contact?.Phone ?? ""),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                Status = (request.CourseEnrollment?.Status ?? (ApiLearning.CourseEnrollmentStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "SubmitCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.EstimateInterviewResponse> SendRejectCourseEnrollment3Async(ApiRecruiting.EstimateInterviewRequest request, CancellationToken token)
    {
        return await _courseEnrollmentApprovalEndpointSendRejectCourseEnrollment3Port.SendAsync(request, token);
    }
}
