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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed partial class CalibrationApprovalEndpoint
{
    /// <summary>
    /// Approves a calibration.
    /// </summary>
    public override async Task<ApiPerformance.ApproveCalibrationResponse> ApproveCalibration(ApiPerformance.ApproveCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CountUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.CountUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiIdentity.CountUsersRequest>();
        using var rpc0 = _identityUserQueryService.CountUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CountJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.CountJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.Calibration?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                {
                    ApiPerformance.Calibration.OriginOneofCase.Customer => (request.Calibration?.Customer?.ReviewId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningQueryService.CountJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.ApproveCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Subject = (request.Calibration?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ApproveCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a calibration.
    /// </summary>
    public override async Task<ApiPerformance.CommentCalibrationApprovalResponse> CommentCalibrationApproval(ApiPerformance.CommentCalibrationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentCalibrationApproval for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.GenerateVenueReportRequest();
        input0.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiCorporateevents.GenerateVenueReportRequest.Types.Details
        {
            Filters = { (request.Calibration?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        input0.Reason = (request.Reason ?? "");
        using var rpc0 = _corporateeventsVenueReportingService.GenerateVenueReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.CommentCalibrationApprovalResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = (request.Calibration?.Assignee ?? new ApiIdentity.UserRef()),
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.CommentCalibrationApprovalResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "CommentCalibrationApproval", (request.Details?.Comment ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.DelegateCalibrationApprovalResponse> DelegateCalibrationApproval(ApiPerformance.DelegateCalibrationApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateCalibrationApproval for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiBroadcasts.CloneBroadcastRequest
        {
            Broadcast = new ApiBroadcasts.Broadcast
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                    UserId = (request.Calibration?.Assignee?.UserId ?? ""),
                },
                Participants = { new ApiBroadcasts.BroadcastParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Calibration?.Assignee?.Email ?? ""),
                        Phone = (request.Calibration?.Requester?.Phone ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _broadcastsBroadcastService.CloneBroadcastAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.CancelBuildingWorkflowRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedBy = (reply0.Broadcast?.Audit?.UpdatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                            {
                                ApiPerformance.Calibration.OriginOneofCase.Customer => (request.Calibration?.Customer?.DisplayName ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _workplacesBuildingWorkflowService.CancelBuildingWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.EscalateReviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.EscalateReviewRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Calibration?.Requester?.Email ?? ""),
                },
            },
            Review = new ApiPerformance.Review
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.Building?.Location?.Recipient ?? ""),
                    Region = (reply0.Broadcast?.Location?.Region ?? ""),
                },
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (request.Calibration ?? new ApiPerformance.Calibration()).OriginCase switch
                    {
                        ApiPerformance.Calibration.OriginOneofCase.Customer => (request.Calibration?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _performanceReviewApprovalService.EscalateReviewAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.DelegateCalibrationApprovalResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Building?.Manager?.DisplayName ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Outcome = new ApiPerformance.DelegateCalibrationApprovalResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "DelegateCalibrationApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a calibration.
    /// </summary>
    public override async Task<ApiPerformance.EscalateCalibrationResponse> EscalateCalibration(ApiPerformance.EscalateCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.RecalculateCandidatesRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Calibration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Calibration?.Requester?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _recruitingCandidateCalculationService.RecalculateCandidatesAsync);
        var reply1 = new ApiIdentity.CountUsersResponse();
        foreach (var item in (request.Calibration?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.CalibrationComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiIdentity.CountUsersRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiIdentity.CountUsersRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (reply0.Candidate?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                    Region = (reply0.Candidate?.PrimaryAddress?.Region ?? ""),
                },
            };
            using var rpc1 = _identityUserQueryService.CountUsersAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiPerformance.SubmitFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.SubmitFeedbackRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (reply0.Candidate?.Contact?.Email ?? ""),
                    UserId = (reply0.Candidate?.RelationshipManager?.UserId ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _performanceFeedbackApprovalService.SubmitFeedbackAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.LintOffersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.LintOffersRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiRecruiting.LintOffersRequest, ApiRecruiting.LintOffersResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".OfferValidationService", "LintOffers",
            Wire.Marshaller<ApiRecruiting.LintOffersRequest>(), Wire.Marshaller<ApiRecruiting.LintOffersResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.EscalateCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Priority = (request.Calibration?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Candidate?.Contact?.Email ?? ""),
                },
            },
            Outcome = new ApiPerformance.EscalateCalibrationResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceId = (reply2.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "EscalateCalibration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a calibration.
    /// </summary>
    public override async Task<ApiPerformance.GetCalibrationApprovalStatusResponse> GetCalibrationApprovalStatus(ApiPerformance.GetCalibrationApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CalibrationId, "calibration_id");
        _logger.LogInformation("Handling GetCalibrationApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", request.CalibrationId,
            "GetCalibrationApprovalStatus", "accepted", token);
        var input0 = new ApiPerformance.ScheduleCalibrationExpiryRequest
        {
            CalibrationId = (request.CalibrationId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiPerformance.ScheduleCalibrationExpiryRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _performanceCalibrationLifecycleService.ScheduleCalibrationExpiryAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiPerformance.GetCalibrationApprovalStatusRequest, ApiItsm.GenerateChangeRequestReportRequest>(value => new ApiItsm.GenerateChangeRequestReportRequest
        {
            Annotations = { (reply0.Calibration?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.GenerateChangeRequestReportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmChangeRequestReportingService.GenerateChangeRequestReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.GetCalibrationApprovalStatusResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Calibration?.Audit?.Etag ?? ""),
                },
                CalibrationId = (request.CalibrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists calibrations a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListPendingCalibrationsResponse> ListPendingCalibrations(ApiPerformance.ListPendingCalibrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingCalibrations for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "calibration", "batch",
            "ListPendingCalibrations", "accepted", token);
        var input0 = new ApiPerformance.PauseGoalWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.PauseGoalWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                History = { new ApiPerformance.GoalStatusChange
                {
                    ChangedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
                Subject = (request.Filter?.OwnerId ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _performanceGoalWorkflowService.PauseGoalWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.ListPendingCalibrationsResponse
        {
            Calibrations = { new ApiPerformance.Calibration
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply0.Goal?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "ListPendingCalibrations", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a calibration.
    /// </summary>
    public override async Task<ApiPerformance.SubmitCalibrationResponse> SubmitCalibration(ApiPerformance.SubmitCalibrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitCalibration for {Tenant}", requestContext.TenantId);
        if (request.Calibration is { } record)
        {
            var snapshot = CalibrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiRecruiting.EstimateInterviewResponse();
        foreach (var item in (request.Calibration?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.CalibrationComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiRecruiting.EstimateInterviewRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Interview = new ApiRecruiting.Interview
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Participants = { new ApiRecruiting.InterviewParticipant
                    {
                        Contact = new ApiCommon.ContactInfo
                        {
                            Email = (item.AuthorEmail ?? ""),
                            PreferredChannel = (request.Calibration?.Requester?.PreferredChannel ?? ""),
                        },
                    } },
                },
            };
            using var rpc0 = _recruitingInterviewCalculationService.EstimateInterviewAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiRecruiting.ExplainInterviewCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Interview?.Organizer?.Email ?? ""),
                    },
                } },
            },
        };
        var reply1 = await SendSubmitCalibration1Async(input1, token);
        var input2 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (reply1.Interview?.Window ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _documentsDocumentExportService.ScheduleDocumentExportAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.SubmitCalibrationResponse
        {
            Calibration = new ApiPerformance.Calibration
            {
                CalibrationId = (request.CalibrationId ?? ""),
                Subject = (request.Calibration?.Subject ?? ""),
            },
            Outcome = new ApiPerformance.SubmitCalibrationResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Comment ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "calibration",
            "audit", "SubmitCalibration", (request.Details?.Comment ?? ""), token);
        return response;
    }
    private async Task<ApiRecruiting.ExplainInterviewCalculationResponse> SendSubmitCalibration1Async(ApiRecruiting.ExplainInterviewCalculationRequest request, CancellationToken token)
    {
        return await _calibrationApprovalEndpointSendSubmitCalibration1Port.SendAsync(request, token);
    }
}
