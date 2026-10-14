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
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed partial class CertificationWorkflowEndpoint
{
    /// <summary>
    /// Advances a certification.
    /// </summary>
    public override async Task<ApiLearning.AdvanceCertificationWorkflowResponse> AdvanceCertificationWorkflow(ApiLearning.AdvanceCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CreateCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Certification?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Certification?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _learningCourseService.CreateCourseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.AdvanceCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Course?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                CertificationId = (request.CertificationId ?? ""),
                Status = (request.Certification?.Status ?? (ApiLearning.CertificationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "AdvanceCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a certification.
    /// </summary>
    public override async Task<ApiLearning.CancelCertificationWorkflowResponse> CancelCertificationWorkflow(ApiLearning.CancelCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.PreviewCourseEnrollmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Card = new ApiLearning.CourseEnrollmentCardFunding
                {
                    HolderName = (request.Certification ?? new ApiLearning.Certification()).PaymentCase switch
                    {
                        ApiLearning.Certification.SettlementOneofCase.Card => (request.Certification?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (request.Certification ?? new ApiLearning.Certification()).PaymentCase switch
                    {
                        ApiLearning.Certification.SettlementOneofCase.Card => (request.Certification?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
            Details = new ApiLearning.PreviewCourseEnrollmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _learningCourseEnrollmentValidationService.PreviewCourseEnrollmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.WatchDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Notify?.Email ?? ""),
                PreferredChannel = (reply0.CourseEnrollment?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _documentsDocumentService.WatchDocuments(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDocuments frame {Type}", change1.GetType().Name);
        }
        var response = new ApiLearning.CancelCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply0.CourseEnrollment?.Holder?.CandidateId ?? ""),
                },
                IssuedAt = (request.Certification?.IssuedAt ?? new Timestamp()),
            },
            Outcome = new ApiLearning.CancelCertificationWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "CancelCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a certification.
    /// </summary>
    public override async Task<ApiLearning.InspectCertificationWorkflowResponse> InspectCertificationWorkflow(ApiLearning.InspectCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CertificationId, "certification_id");
        _logger.LogInformation("Handling InspectCertificationWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "certification", request.CertificationId,
            "InspectCertificationWorkflow", "accepted", token);
        var input0 = new ApiLearning.CalculateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _learningCertificationCalculationService.CalculateCertificationAsync);
        var response = new ApiLearning.InspectCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Total = (reply0.Certification?.Total ?? new ApiCommon.Money()),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists certifications a page at a time.
    /// </summary>
    public override async Task<ApiLearning.ListCertificationWorkflowsResponse> ListCertificationWorkflows(ApiLearning.ListCertificationWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListCertificationWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "certification", "batch",
            "ListCertificationWorkflows", "accepted", token);
        var input0 = new ApiLearning.DelegateCertificationApprovalRequest
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendListCertificationWorkflows0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.ListCertificationWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ListCertificationWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a certification.
    /// </summary>
    public override async Task<ApiLearning.PauseCertificationWorkflowResponse> PauseCertificationWorkflow(ApiLearning.PauseCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiRecruiting.EstimateOfferResponse();
        foreach (var item in (request.Certification?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationLine>()))
        {
            var input0 = new ApiRecruiting.EstimateOfferRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Offer = new ApiRecruiting.Offer
                {
                    Lines = { new ApiRecruiting.OfferLine
                    {
                        Description = (item.Description ?? ""),
                        Tax = new ApiRecruiting.OfferLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                    } },
                },
            };
            using var rpc0 = _recruitingOfferCalculationService.EstimateOfferAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var reply1 = new ApiInvoicing.CreateInvoiceBatchResponse();
        foreach (var item in (request.Certification?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiInvoicing.CreateInvoiceBatchRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                InvoiceBatch = new ApiInvoicing.InvoiceBatch
                {
                    Lines = { new ApiInvoicing.InvoiceBatchLine
                    {
                        Tax = new ApiInvoicing.InvoiceBatchLineTax
                        {
                            Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                },
            };
            using var rpc1 = _invoicingInvoiceBatchService.CreateInvoiceBatchAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiLearning.PauseCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Number = (request.Certification?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "PauseCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a certification.
    /// </summary>
    public override async Task<ApiLearning.ResumeCertificationWorkflowResponse> ResumeCertificationWorkflow(ApiLearning.ResumeCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Certification?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationAttachment>()).Select(async item =>
        {
            var input0 = new ApiRecruiting.NormalizeOfferRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Offer = new ApiRecruiting.Offer
                {
                    Attachments = { new ApiRecruiting.OfferAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        ContentType = (item.ContentType ?? ""),
                        SizeBytes = item.SizeBytes,
                    } },
                },
            };
            using var rpc0 = _recruitingOfferValidationService.NormalizeOfferAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiRecruiting.NormalizeOfferResponse();
        var reply1 = new ApiRecruiting.QuoteJobOpeningResponse();
        foreach (var item in (request.Certification?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationAttachment>()))
        {
            var input1 = new ApiRecruiting.QuoteJobOpeningRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        Checksum = (item.Checksum ?? ""),
                        ContentType = (item.ContentType ?? ""),
                        SizeBytes = item.SizeBytes,
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc1 = _recruitingJobOpeningCalculationService.QuoteJobOpeningAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiLearning.RetryCourseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Offer?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _learningCourseWorkflowService.RetryCourseWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.ResumeCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                BillingAddress = (request.Certification?.BillingAddress ?? new ApiCommon.PostalAddress()),
                CertificationId = (request.CertificationId ?? ""),
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply2.Course?.Owner?.DisplayName ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "ResumeCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a certification.
    /// </summary>
    public override async Task<ApiLearning.RetryCertificationWorkflowResponse> RetryCertificationWorkflow(ApiLearning.RetryCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RetryCertificationWorkflowToLintCandidates0();
        using var rpc0 = _recruitingCandidateValidationService.LintCandidatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.RetryCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Number = (request.Certification?.Number ?? ""),
            },
            Outcome = new ApiLearning.RetryCertificationWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "RetryCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a certification.
    /// </summary>
    public override async Task<ApiLearning.SignalCertificationWorkflowResponse> SignalCertificationWorkflow(ApiLearning.SignalCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.BrowseDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.BrowseDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.Certification?.IssuedAt ?? new Timestamp()),
                Labels = { (request.Certification?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Certification?.Counterparty?.CandidateId ?? ""),
                Region = (request.Certification?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await SendSignalCertificationWorkflow0Async(input0, token);
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.BrowseDocumentFoldersRequest>("acme.learning.v1.CertificationWorkflowService/SignalCertificationWorkflow/0", input0.Clone()), token);
        var response = new ApiLearning.SignalCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                CertificationId = (request.CertificationId ?? ""),
                Number = (request.Certification?.Number ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "SignalCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a certification.
    /// </summary>
    public override async Task<ApiLearning.StartCertificationWorkflowResponse> StartCertificationWorkflow(ApiLearning.StartCertificationWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartCertificationWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Certification is { } record)
        {
            var snapshot = CertificationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CreateCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Certification?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Certification?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Certification?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _learningCourseService.CreateCourseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.RejectCourseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    PostalCode = (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _learningCourseApprovalService.RejectCourseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiTravel.EscalateTripResponse();
        foreach (var item in (request.Certification?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiLearning.CertificationLine>()))
        {
            var input2 = new ApiTravel.EscalateTripRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Trip = new ApiTravel.Trip
                {
                    Lines = { new ApiTravel.TripLine
                    {
                        Amount = (item.Tax?.Amount ?? new ApiCommon.Money()),
                        Tax = new ApiTravel.TripLineTax
                        {
                            Amount = (item.Amount ?? new ApiCommon.Money()),
                            Exemptions = { (item.Tax?.Exemptions ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                },
            };
            using var rpc2 = _travelTripApprovalService.EscalateTripAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiLearning.StartCertificationWorkflowResponse
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Course?.Audit?.Etag ?? ""),
                },
                CertificationId = (request.CertificationId ?? ""),
                Status = (request.Certification?.Status ?? (ApiLearning.CertificationStatus)0),
            },
            Outcome = new ApiLearning.StartCertificationWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "certification",
            "audit", "StartCertificationWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.BrowseDocumentFoldersResponse> SendSignalCertificationWorkflow0Async(ApiDocuments.BrowseDocumentFoldersRequest request, CancellationToken token)
    {
        return await _certificationWorkflowEndpointSendSignalCertificationWorkflow0Port.SendAsync(request, token);
    }
}
