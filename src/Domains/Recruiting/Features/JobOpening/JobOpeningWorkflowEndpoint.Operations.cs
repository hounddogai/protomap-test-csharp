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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class JobOpeningWorkflowEndpoint
{
    /// <summary>
    /// Advances a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.AdvanceJobOpeningWorkflowResponse> AdvanceJobOpeningWorkflow(ApiRecruiting.AdvanceJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RejectDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.JobOpening?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.JobOpening?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiDevices.RejectDevicePolicyRequest, ApiDevices.RejectDevicePolicyResponse>(
            MethodType.Unary, "acme.devices.v1" + ".DevicePolicyApprovalService", "RejectDevicePolicy",
            Wire.Marshaller<ApiDevices.RejectDevicePolicyRequest>(), Wire.Marshaller<ApiDevices.RejectDevicePolicyResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.CountVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.CountVenuesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var command1 = new DispatchCommand<ApiVenues.CountVenuesRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _venuesVenueQueryService.CountVenuesAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.AdvanceJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                IssuedAt = (request.JobOpening?.IssuedAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "AdvanceJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.CancelJobOpeningWorkflowResponse> CancelJobOpeningWorkflow(ApiRecruiting.CancelJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiRecruiting.CancelJobOpeningWorkflowRequest, ApiItsm.EscalateServiceAssetRequest>(value => new ApiItsm.EscalateServiceAssetRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.EscalateServiceAssetRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (value.Details?.Notify?.Email ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (value.JobOpening?.BillingAddress?.Locality ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (value.JobOpening?.Audit?.Etag ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _itsmServiceAssetApprovalService.EscalateServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.QuoteEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.CancelJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.ServiceAsset?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CancelJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.InspectJobOpeningWorkflowResponse> InspectJobOpeningWorkflow(ApiRecruiting.InspectJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling InspectJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "InspectJobOpeningWorkflow", "accepted", token);
        var input0 = request.InspectJobOpeningWorkflowToCommentCourseEnrollmentApproval0();
        using var rpc0 = _learningCourseEnrollmentApprovalService.CommentCourseEnrollmentApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.InspectJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists job openings a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListJobOpeningWorkflowsResponse> ListJobOpeningWorkflows(ApiRecruiting.ListJobOpeningWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListJobOpeningWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "ListJobOpeningWorkflows", "accepted", token);
        var input0 = new ApiDocuments.ShareDocumentFolderExportRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareDocumentFolderExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Variables = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentFolderExportService.ShareDocumentFolderExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ListJobOpeningWorkflowsResponse
        {
            JobOpenings = { new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DocumentFolder?.Audit?.Etag ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ListJobOpeningWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.PauseJobOpeningWorkflowResponse> PauseJobOpeningWorkflow(ApiRecruiting.PauseJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ReconcileInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ReconcileInterviewsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Notify?.Email ?? ""),
                        Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _recruitingInterviewSyncService.ReconcileInterviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExpireInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExpireInterviewRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.JobOpening?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                    UserId = (reply0.Interview?.Organizer?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            InterviewId = (request.JobOpening?.Counterparty?.InterviewId ?? ""),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiRecruiting.ExpireInterviewRequest>();
        using var rpc1 = _recruitingInterviewLifecycleService.ExpireInterviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.PauseJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Interview?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = (request.JobOpening?.BillingAddress ?? new ApiCommon.PostalAddress()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "PauseJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.ResumeJobOpeningWorkflowResponse> ResumeJobOpeningWorkflow(ApiRecruiting.ResumeJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiEmployees.CheckEmploymentContractEligibilityResponse();
        foreach (var item in (request.JobOpening?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiRecruiting.JobOpeningAttachment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiEmployees.CheckEmploymentContractEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Attachments = { new ApiEmployees.EmploymentContractAttachment
                    {
                        ContentType = (item.ContentType ?? ""),
                    } },
                },
            };
            using var rpc0 = _employeesEmploymentContractValidationService.CheckEmploymentContractEligibilityAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiDocuments.PullDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PullDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentSyncService.PullDocumentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.ResumeJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.Document?.BillingAddress?.CountryCode ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Total = (request.JobOpening?.Total ?? new ApiCommon.Money()),
            },
            Outcome = new ApiRecruiting.ResumeJobOpeningWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ResumeJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.RetryJobOpeningWorkflowResponse> RetryJobOpeningWorkflow(ApiRecruiting.RetryJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CommentCourseEnrollmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CourseEnrollment = new ApiLearning.CourseEnrollment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.JobOpening?.Audit?.CreatedBy ?? ""),
                },
                Card = new ApiLearning.CourseEnrollmentCardFunding
                {
                    Network = (request.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (request.JobOpening?.Card?.Network ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _learningCourseEnrollmentApprovalService.CommentCourseEnrollmentApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.RetryJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (reply0.CourseEnrollment?.Holder?.DisplayName ?? ""),
                },
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "RetryJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.SignalJobOpeningWorkflowResponse> SignalJobOpeningWorkflow(ApiRecruiting.SignalJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ScheduleJobOpeningExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningLifecycleService.ScheduleJobOpeningExpiryAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLearning.ListCourseWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLearning.ListCourseWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _learningCourseWorkflowService.ListCourseWorkflowsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.SignalJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                DueAt = (request.JobOpening?.DueAt ?? new Timestamp()),
                JobOpeningId = (request.JobOpeningId ?? ""),
                Metadata = { (reply0.JobOpening?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SignalJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.StartJobOpeningWorkflowResponse> StartJobOpeningWorkflow(ApiRecruiting.StartJobOpeningWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartJobOpeningWorkflow for {Tenant}", requestContext.TenantId);
        if (request.JobOpening is { } record)
        {
            var snapshot = JobOpeningMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.SignalTravelPolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SignalTravelPolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.JobOpening?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _travelTravelPolicyWorkflowService.SignalTravelPolicyWorkflowAsync);
        var input1 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _sendStartJobOpeningWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.ExplainOfferCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ExplainOfferCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.TravelPolicy?.Owner?.Email ?? ""),
                },
            },
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient>("acme.recruiting.v1.OfferCalculationService");
        using var rpc2 = named2.ExplainOfferCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiAuthorization.CreateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.JobOpening?.Audit?.CreatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor3 = new Method<ApiAuthorization.CreateAccessPolicyRequest, ApiAuthorization.CreateAccessPolicyResponse>(
            MethodType.Unary, "acme.authorization.v1" + ".AccessPolicyService", "CreateAccessPolicy",
            Wire.Marshaller<ApiAuthorization.CreateAccessPolicyRequest>(), Wire.Marshaller<ApiAuthorization.CreateAccessPolicyResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiRecruiting.StartJobOpeningWorkflowResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.AccessPolicy?.Audit?.Etag ?? ""),
                },
                JobOpeningId = (request.JobOpeningId ?? ""),
                Status = (request.JobOpening?.Status ?? (ApiRecruiting.JobOpeningStatus)0),
            },
            Outcome = new ApiRecruiting.StartJobOpeningWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply2.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "StartJobOpeningWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
