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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTicketing = Acme.Ticketing.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CourseEnrollmentCalculationEndpoint
{
    /// <summary>
    /// Calculates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.CalculateCourseEnrollmentResponse> CalculateCourseEnrollment(ApiLearning.CalculateCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.QuoteCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Card = new ApiLearning.CertificationCardSettlement
                {
                    Network = (request.CourseEnrollment ?? new ApiLearning.CourseEnrollment()).PaymentCase switch
                    {
                        ApiLearning.CourseEnrollment.FundingOneofCase.Card => (request.CourseEnrollment?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _learningCertificationCalculationService.QuoteCertificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.CalculateCourseEnrollmentResponse
        {
            Breakdown = new ApiLearning.CalculateCourseEnrollmentResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Certification?.Audit?.CreatedBy ?? ""),
                },
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "CalculateCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.CompareCourseEnrollmentOptionsResponse> CompareCourseEnrollmentOptions(ApiLearning.CompareCourseEnrollmentOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareCourseEnrollmentOptions for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SuggestDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.SuggestDevicePoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.CourseEnrollment?.OpenedAt ?? new Timestamp()),
                Labels = { (request.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyQueryService.SuggestDevicePoliciesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLearning.CompareCourseEnrollmentOptionsResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                OpenedAt = (request.CourseEnrollment?.OpenedAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "CompareCourseEnrollmentOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.EstimateCourseEnrollmentResponse> EstimateCourseEnrollment(ApiLearning.EstimateCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CheckSlaPolicyEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.CourseEnrollment?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyValidationService.CheckSlaPolicyEligibilityAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLearning.QuoteCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Bank = new ApiLearning.CertificationBankSettlement
                {
                    Bic = (request.CourseEnrollment ?? new ApiLearning.CourseEnrollment()).PaymentCase switch
                    {
                        ApiLearning.CourseEnrollment.FundingOneofCase.Bank => (request.CourseEnrollment?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.CourseEnrollment?.Holder?.DisplayName ?? ""),
                    Email = (request.CourseEnrollment?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _learningCertificationCalculationService.QuoteCertificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.EstimateCourseEnrollmentResponse
        {
            Breakdown = new ApiLearning.EstimateCourseEnrollmentResponse.Types.Breakdown
            {
                CurrencyCode = (request.CourseEnrollment?.CurrencyCode ?? ""),
            },
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "EstimateCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.ExplainCourseEnrollmentCalculationResponse> ExplainCourseEnrollmentCalculation(ApiLearning.ExplainCourseEnrollmentCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainCourseEnrollmentCalculation for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.ScoreLearningPathRequest
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
                            CreatedBy = (request.CourseEnrollment?.Audit?.CreatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.CourseEnrollment?.Holder?.DisplayName ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.CourseEnrollment?.Contact?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _sendExplainCourseEnrollmentCalculation0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.SimulateLearningPathRequest
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
                            UpdatedBy = (reply0.LearningPath?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
        };
        var named1 = _registry.Get<ApiLearning.LearningPathCalculationService.LearningPathCalculationServiceClient>("acme.learning.v1.LearningPathCalculationService");
        using var rpc1 = named1.SimulateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.ExplainCourseEnrollmentCalculationResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Contact = (request.Details?.Contact ?? new ApiCommon.ContactInfo()),
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Outcome = new ApiLearning.ExplainCourseEnrollmentCalculationResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "ExplainCourseEnrollmentCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.GetCourseEnrollmentRatesResponse> GetCourseEnrollmentRates(ApiLearning.GetCourseEnrollmentRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CourseEnrollmentId, "course_enrollment_id");
        _logger.LogInformation("Handling GetCourseEnrollmentRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment", request.CourseEnrollmentId,
            "GetCourseEnrollmentRates", "accepted", token);
        var response = new ApiLearning.GetCourseEnrollmentRatesResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.QuoteCourseEnrollmentResponse> QuoteCourseEnrollment(ApiLearning.QuoteCourseEnrollmentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteCourseEnrollment for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiLearning.PreviewLearningPathResponse();
        foreach (var item in (request.CourseEnrollment?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CourseEnrollmentLimit>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiLearning.PreviewLearningPathRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                LearningPath = new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Children = { new ApiLearning.LearningPath
                        {
                            Children = { new ApiLearning.LearningPath
                            {
                                Name = (item.Name ?? ""),
                            } },
                        } },
                    } },
                },
            };
            using var rpc0 = _learningLearningPathValidationService.PreviewLearningPathAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiTicketing.UpdateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiTicketing.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.CourseEnrollment?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var command1 = new DispatchCommand<ApiTicketing.UpdateTicketRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _ticketingTicketService.UpdateTicketAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.QuoteCourseEnrollmentResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
                OpenedAt = (request.CourseEnrollment?.OpenedAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "QuoteCourseEnrollment", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a course enrollment.
    /// </summary>
    public override async Task<ApiLearning.RecalculateCourseEnrollmentsResponse> RecalculateCourseEnrollments(ApiLearning.RecalculateCourseEnrollmentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateCourseEnrollments for {Tenant}", requestContext.TenantId);
        if (request.CourseEnrollment is { } record)
        {
            var snapshot = CourseEnrollmentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiLearning.RecalculateCourseEnrollmentsRequest, ApiAuthorization.SummarizeAccessPoliciesRequest>(value => new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.SummarizeAccessPoliciesRequest.Types.Details
            {
                Filters = { (value.CourseEnrollment?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (value.Reason ?? ""),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RevertDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.CourseEnrollment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentHistoryService.RevertDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.RecalculateCourseEnrollmentsResponse
        {
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = (request.CourseEnrollment?.Contact ?? new ApiCommon.ContactInfo()),
                CourseEnrollmentId = (request.CourseEnrollmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Destination?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "course_enrollment",
            "audit", "RecalculateCourseEnrollments", (request.Details?.Destination?.Region ?? ""), token);
        return response;
    }
}
