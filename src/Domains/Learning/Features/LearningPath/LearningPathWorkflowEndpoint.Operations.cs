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
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Learning;

public sealed partial class LearningPathWorkflowEndpoint
{
    /// <summary>
    /// Advances a learning path.
    /// </summary>
    public override async Task<ApiLearning.AdvanceLearningPathWorkflowResponse> AdvanceLearningPathWorkflow(ApiLearning.AdvanceLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.ExplainCourseDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LearningPath?.Manager?.DisplayName ?? ""),
                    Email = (request.Details?.Notify?.Email ?? ""),
                    UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _learningCourseValidationService.ExplainCourseDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.AdvanceLearningPathWorkflowToDelegateCertificationApproval1(reply0);
        using var rpc1 = _learningCertificationApprovalService.DelegateCertificationApprovalAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.AdvanceLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply1.Certification?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
                Depth = (request.LearningPath?.Depth ?? 0),
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Certification?.BillingAddress?.PostalCode ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "AdvanceLearningPathWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a learning path.
    /// </summary>
    public override async Task<ApiLearning.CancelLearningPathWorkflowResponse> CancelLearningPathWorkflow(ApiLearning.CancelLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.CancelCourseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.LearningPath?.Location?.PostalCode ?? ""),
                },
            },
            Details = new ApiLearning.CancelCourseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendCancelLearningPathWorkflow0Async(input0, token);
        var input1 = new ApiLearning.SubmitCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Course?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.Course ?? new ApiLearning.Course()).LocatorCase switch
                    {
                        ApiLearning.Course.LocatorOneofCase.Address => (reply0.Course?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.SubmitCertificationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.LearningPath?.Manager?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _learningCertificationApprovalService.SubmitCertificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.CancelLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.Course?.Audit?.CreatedBy ?? ""),
                    },
                    Location = new ApiCommon.PostalAddress
                    {
                        Recipient = (reply1.Certification?.BillingAddress?.Recipient ?? ""),
                    },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (reply1.Certification?.Counterparty?.Email ?? ""),
                    },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Manager = (request.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "CancelLearningPathWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a learning path.
    /// </summary>
    public override async Task<ApiLearning.InspectLearningPathWorkflowResponse> InspectLearningPathWorkflow(ApiLearning.InspectLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LearningPathId, "learning_path_id");
        _logger.LogInformation("Handling InspectLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", request.LearningPathId,
            "InspectLearningPathWorkflow", "accepted", token);
        var response = new ApiLearning.InspectLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                LearningPathId = (request.LearningPathId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists learning paths a page at a time.
    /// </summary>
    public override async Task<ApiLearning.ListLearningPathWorkflowsResponse> ListLearningPathWorkflows(ApiLearning.ListLearningPathWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListLearningPathWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "ListLearningPathWorkflows", "accepted", token);
        var response = new ApiLearning.ListLearningPathWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ListLearningPathWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a learning path.
    /// </summary>
    public override async Task<ApiLearning.PauseLearningPathWorkflowResponse> PauseLearningPathWorkflow(ApiLearning.PauseLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SimulateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.LearningPath?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.LearningPath?.Manager?.Email ?? ""),
                    },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiRecruiting.SimulateInterviewRequest>();
        using var rpc0 = _recruitingInterviewCalculationService.SimulateInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.NormalizeLearningPathRequest
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
                            UserId = (reply0.Interview?.Organizer?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiLearning.NormalizeLearningPathRequest>(json1);
        using var rpc1 = _learningLearningPathValidationService.NormalizeLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.PauseLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Depth = (request.LearningPath?.Depth ?? 0),
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Interview?.Location?.PostalCode ?? ""),
                    Recipient = (reply1.LearningPath?.Location?.Recipient ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "PauseLearningPathWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a learning path.
    /// </summary>
    public override async Task<ApiLearning.ResumeLearningPathWorkflowResponse> ResumeLearningPathWorkflow(ApiLearning.ResumeLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ApproveCalibrationRequest();
        input0.Calibration = new ApiPerformance.Calibration
        {
            Customer = new ApiPerformance.ReviewRef
            {
                DisplayName = (request.LearningPath?.Manager?.DisplayName ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _performanceCalibrationApprovalService.ApproveCalibrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ResumeManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ResumeManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.LearningPath?.Location?.Recipient ?? ""),
                    Region = (request.LearningPath?.Location?.Region ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _devicesManagedDeviceWorkflowService.ResumeManagedDeviceWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.CommentServiceAssetApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply1.ManagedDevice?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Lines = { (reply1.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply1.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    PostalCode = (reply1.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply1.ManagedDevice?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _itsmServiceAssetApprovalService.CommentServiceAssetApprovalAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.RecallEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskEscalationApprovalService.RecallEscalationAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiLearning.ResumeLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply1.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply2.ServiceAsset?.Address?.Locality ?? ""),
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Manager = (request.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ResumeLearningPathWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a learning path.
    /// </summary>
    public override async Task<ApiLearning.RetryLearningPathWorkflowResponse> RetryLearningPathWorkflow(ApiLearning.RetryLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLearning.RejectLearningPathRequest
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
                            Manager = (request.LearningPath?.Manager ?? new ApiIdentity.UserRef()),
                        } },
                    } },
                } },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _learningLearningPathApprovalService.RejectLearningPathAsync);
        var input1 = new ApiHelpdesk.RejectTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RejectTicketRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.LearningPath?.Manager?.Email ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.LearningPath?.Manager?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendRetryLearningPathWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.LintInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintInterviewsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (reply1.Ticket?.Requester?.PreferredChannel ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.LearningPath?.Location?.CountryCode ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Notify?.Email ?? ""),
                    },
                } },
            },
        };
        var named2 = _registry.Get<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>("acme.recruiting.v1.InterviewValidationService");
        using var rpc2 = named2.LintInterviewsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiLearning.RetryLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Children = { new ApiLearning.LearningPath
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply2.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (reply1.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        Region = (reply0.LearningPath?.Location?.Region ?? ""),
                    },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = (request.LearningPath?.Location ?? new ApiCommon.PostalAddress()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "RetryLearningPathWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a learning path.
    /// </summary>
    public override async Task<ApiLearning.StartLearningPathWorkflowResponse> StartLearningPathWorkflow(ApiLearning.StartLearningPathWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartLearningPathWorkflow for {Tenant}", requestContext.TenantId);
        if (request.LearningPath is { } record)
        {
            var snapshot = LearningPathMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.RejectEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RejectEscalationRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.LearningPath?.Audit?.Etag ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiHelpdesk.RejectEscalationRequest, ApiHelpdesk.RejectEscalationResponse>(
            MethodType.Unary, "acme.helpdesk.v1" + ".EscalationApprovalService", "RejectEscalation",
            Wire.Marshaller<ApiHelpdesk.RejectEscalationRequest>(), Wire.Marshaller<ApiHelpdesk.RejectEscalationResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.RecalculateJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.LearningPath?.Location?.Region ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiRecruiting.RecalculateJobOpeningsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _recruitingJobOpeningCalculationService.RecalculateJobOpeningsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.StartLearningPathWorkflowResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Attributes = { (request.LearningPath?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (reply1.JobOpening?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.Escalation?.Assignee?.DisplayName ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        DisplayName = (reply0.Escalation?.Assignee?.DisplayName ?? ""),
                    },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "StartLearningPathWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLearning.CancelCourseWorkflowResponse> SendCancelLearningPathWorkflow0Async(ApiLearning.CancelCourseWorkflowRequest request, CancellationToken token)
    {
        return await _learningPathWorkflowEndpointSendCancelLearningPathWorkflow0Port.SendAsync(request, token);
    }
}
