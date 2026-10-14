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
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLearning = Acme.Learning.V1;
using ApiMedia = Acme.Media.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CertificationApprovalEndpoint
{
    /// <summary>
    /// Approves a certification.
    /// </summary>
    public override async Task<ApiLearning.ApproveCertificationResponse> ApproveCertification(ApiLearning.ApproveCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMedia.FindMediaAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMedia.FindMediaAssetsRequest.Types.Filter
            {
                CreatedAfter = (request.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Certification?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Certification?.Counterparty?.CandidateId ?? ""),
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiMedia.FindMediaAssetsRequest>(json0);
        using var rpc0 = _mediaMediaAssetQueryService.FindMediaAssetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.ApproveCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
            },
            Outcome = new ApiLearning.ApproveCertificationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ApproveCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a certification.
    /// </summary>
    public override async Task<ApiLearning.CommentCertificationApprovalResponse> CommentCertificationApproval(ApiLearning.CommentCertificationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentCertificationApproval for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.RejectLearningPathRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.LearningPath = new ApiLearning.LearningPath
        {
            Children = { new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Certification?.Audit?.UpdatedBy ?? ""),
                },
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (request.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                } },
            } },
        };
        using var rpc0 = _learningLearningPathApprovalService.RejectLearningPathAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.PreviewInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.LearningPath?.Location?.PostalCode ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.LearningPath?.Manager?.Email ?? ""),
                    },
                } },
            },
        };
        using var rpc1 = _recruitingInterviewValidationService.PreviewInterviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.CommentCertificationApprovalResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (reply1.Interview?.Organizer?.Email ?? ""),
                },
                Metadata = { (request.Certification?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "CommentCertificationApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a certification.
    /// </summary>
    public override async Task<ApiLearning.DelegateCertificationApprovalResponse> DelegateCertificationApproval(ApiLearning.DelegateCertificationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateCertificationApproval for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.PauseCourseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Certification?.BillingAddress?.PostalCode ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _learningCourseWorkflowService.PauseCourseWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.ExplainJobOpeningCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningCalculationService.ExplainJobOpeningCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.DelegateCertificationApprovalResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Course?.Audit?.UpdatedBy ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
                Metadata = { (request.Certification?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "DelegateCertificationApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a certification.
    /// </summary>
    public override async Task<ApiLearning.EscalateCertificationResponse> EscalateCertification(ApiLearning.EscalateCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.NormalizeInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _recruitingInterviewValidationService.NormalizeInterviewAsync);
        var input1 = new ApiFeatureflags.PreviewFeatureFlagRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.PreviewFeatureFlagRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Interview?.Organizer?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendEscalateCertification1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLearning.BrowseLearningPathsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.BrowseLearningPathsRequest.Types.Filter
            {
                CreatedAfter = (reply1.FeatureFlag?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply0.Interview?.Location?.Region ?? ""),
            },
        };
        var named2 = _registry.Get<ApiLearning.LearningPathQueryService.LearningPathQueryServiceClient>("acme.learning.v1.LearningPathQueryService");
        using var rpc2 = named2.BrowseLearningPathsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.EscalateCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.FeatureFlag?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = (request.Certification?.BillingAddress ?? new ApiCommon.PostalAddress()),
                CertificationId = (request.CertificationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "EscalateCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a certification.
    /// </summary>
    public override async Task<ApiLearning.GetCertificationApprovalStatusResponse> GetCertificationApprovalStatus(ApiLearning.GetCertificationApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CertificationId, "certification_id");
        _logger.LogInformation("Handling GetCertificationApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "certification", request.CertificationId,
            "GetCertificationApprovalStatus", "accepted", token);
        var input0 = new ApiAuthorization.ValidateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.ValidateAccessPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var descriptor0 = new Method<ApiAuthorization.ValidateAccessPolicyRequest, ApiAuthorization.ValidateAccessPolicyResponse>(
            MethodType.Unary, "acme.authorization.v1" + ".AccessPolicyValidationService", "ValidateAccessPolicy",
            Wire.Marshaller<ApiAuthorization.ValidateAccessPolicyRequest>(), Wire.Marshaller<ApiAuthorization.ValidateAccessPolicyResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.GetCertificationApprovalStatusResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists certifications a page at a time.
    /// </summary>
    public override async Task<ApiLearning.ListPendingCertificationsResponse> ListPendingCertifications(ApiLearning.ListPendingCertificationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingCertifications for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "certification", "batch",
            "ListPendingCertifications", "accepted", token);
        var input0 = new ApiLabeling.ExportLabelingTasksRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLabeling.ExportLabelingTasksRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _labelingLabelingTaskQueryService.ExportLabelingTasks(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLabelingTasks frame {Type}", change0.GetType().Name);
        }
        var response = new ApiLearning.ListPendingCertificationsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ListPendingCertifications", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a certification.
    /// </summary>
    public override async Task<ApiLearning.RecallCertificationResponse> RecallCertification(ApiLearning.RecallCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.RevertDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RevertDocumentTemplateRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Certification?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateHistoryService.RevertDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.SubmitCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.DocumentTemplate?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _learningCourseApprovalService.SubmitCourseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDeliveries.WatchDeliveriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDeliveries.WatchDeliveriesRequest.Types.Filter
            {
                Region = (reply1.Course?.Address?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Certification?.Counterparty?.Email ?? ""),
            },
        };
        using var stream2 = _deliveriesDeliveryService.WatchDeliveries(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDeliveries frame {Type}", change2.GetType().Name);
        }
        var response = new ApiLearning.RecallCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Number = (reply0.DocumentTemplate?.Number ?? ""),
                Status = (request.Certification?.Status ?? (ApiLearning.CertificationStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "RecallCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a certification.
    /// </summary>
    public override async Task<ApiLearning.RejectCertificationResponse> RejectCertification(ApiLearning.RejectCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.CountRoomBookingTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountRoomBookingTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Certification?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Certification?.CertificationId ?? ""),
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc0 = _workplacesRoomBookingReportingService.CountRoomBookingTotalsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.ListManagedDeviceWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListManagedDeviceWorkflowsRequest.Types.Filter
            {
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _devicesManagedDeviceWorkflowService.ListManagedDeviceWorkflowsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiLearning.RejectCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Notes = (request.Certification?.Notes ?? ""),
            },
            Outcome = new ApiLearning.RejectCertificationResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "RejectCertification", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a certification.
    /// </summary>
    public override async Task<ApiLearning.SubmitCertificationResponse> SubmitCertification(ApiLearning.SubmitCertificationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitCertification for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ResolveCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.ResolveCandidatesRequest>();
        using var rpc0 = _recruitingCandidateQueryService.ResolveCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiLearning.AdvanceCourseWorkflowResponse();
        foreach (var item in (request.Certification?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationLine>()))
        {
            var input1 = new ApiLearning.AdvanceCourseWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Course = new ApiLearning.Course
                {
                    Components = { { "primary", new ApiLearning.CourseComponent
                    {
                        Description = (item.Description ?? ""),
                        Quantity = item.Quantity,
                    } } },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (request.Certification?.Counterparty?.Email ?? ""),
                        UserId = (request.Details?.Approver?.UserId ?? ""),
                    },
                },
            };
            using var rpc1 = _learningCourseWorkflowService.AdvanceCourseWorkflowAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiDocuments.RecalculateSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecalculateSignaturesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Certification?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (reply1.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply1.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _documentsSignatureCalculationService.RecalculateSignaturesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLearning.ApproveCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Certification?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Course?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Certification?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.Course?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc3 = _learningCourseApprovalService.ApproveCourseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLearning.ApproveCourseRequest>("acme.learning.v1.CertificationApprovalService/SubmitCertification/3", input3.Clone()), token);
        var response = new ApiLearning.SubmitCertificationResponse
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply3.Course?.Address?.CountryCode ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
            },
            Outcome = new ApiLearning.SubmitCertificationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "SubmitCertification", (request.Reason ?? ""), token);
        return response;
    }
}
