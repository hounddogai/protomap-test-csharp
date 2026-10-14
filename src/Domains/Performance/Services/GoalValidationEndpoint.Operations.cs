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
using ApiCommunity = Acme.Community.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMedia = Acme.Media.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

public sealed partial class GoalValidationEndpoint
{
    /// <summary>
    /// Explains a goal.
    /// </summary>
    public override async Task<ApiPerformance.ExplainGoalDecisionResponse> ExplainGoalDecision(ApiPerformance.ExplainGoalDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainGoalDecision for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.CompareFeedbackOptionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackCalculationService.CompareFeedbackOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.CloneVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply0.Feedback?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _venuesVenueService.CloneVenueAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.NormalizeSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyValidationService.NormalizeSlaPolicyAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.ExplainGoalDecisionResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Venue?.Owner?.UserId ?? ""),
                },
                GoalId = (request.GoalId ?? ""),
                Subject = (request.Goal?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Feedback?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ExplainGoalDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a goal.
    /// </summary>
    public override async Task<ApiPerformance.GetGoalRulesResponse> GetGoalRules(ApiPerformance.GetGoalRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.GoalId, "goal_id");
        _logger.LogInformation("Handling GetGoalRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", request.GoalId,
            "GetGoalRules", "accepted", token);
        var input0 = new ApiPerformance.ExplainFeedbackCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackCalculationRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackCalculationService.ExplainFeedbackCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.GetGoalRulesResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a goal.
    /// </summary>
    public override async Task<ApiPerformance.LintGoalsResponse> LintGoals(ApiPerformance.LintGoalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintGoals for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCommunity.BatchCreateForumsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Record = new ApiCommunity.Forum
            {
                Children = { new ApiCommunity.Forum
                {
                    Children = { new ApiCommunity.Forum
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.Goal?.Audit?.Etag ?? ""),
                            UpdatedAt = (request.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                            UpdatedBy = (request.Goal?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
            },
        };
        using var stream0 = _communityForumService.BatchCreateForums(cancellationToken: token);
        await stream0.RequestStream.WriteAsync(input0, token);
        await stream0.RequestStream.CompleteAsync();
        var reply0 = await stream0.ResponseAsync;
        var input1 = new ApiIdentity.ExplainUserDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ExplainUserDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Goal?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Goal?.Requester?.Phone ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendLintGoals1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.ExplainFeedbackCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.User?.Contact?.Email ?? ""),
                    Name = (reply1.User?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply1.User?.Contact?.PreferredChannel ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Goal?.Assignee?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _performanceFeedbackCalculationService.ExplainFeedbackCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiTravel.ExplainTripCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var descriptor3 = new Method<ApiTravel.ExplainTripCalculationRequest, ApiTravel.ExplainTripCalculationResponse>(
            MethodType.Unary, "acme.travel.v1" + ".TripCalculationService", "ExplainTripCalculation",
            Wire.Marshaller<ApiTravel.ExplainTripCalculationRequest>(), Wire.Marshaller<ApiTravel.ExplainTripCalculationResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.LintGoalsResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply2.Feedback?.Subject?.Email ?? ""),
                    UserId = (reply1.User?.RelationshipManager?.UserId ?? ""),
                },
                Comments = { new ApiPerformance.GoalComment
                {
                    CreatedAt = (reply1.User?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                GoalId = (request.GoalId ?? ""),
                Status = (request.Goal?.Status ?? (ApiPerformance.GoalStatus)0),
            },
            Outcome = new ApiPerformance.LintGoalsResponse.Types.Outcome
            {
                ReferenceId = (reply2.Outcome?.ReferenceId ?? ""),
                Status = (reply3.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "LintGoals", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a goal.
    /// </summary>
    public override async Task<ApiPerformance.NormalizeGoalResponse> NormalizeGoal(ApiPerformance.NormalizeGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ExplainFeedbackCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _performanceFeedbackCalculationService.ExplainFeedbackCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.QuoteBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                            {
                                ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                                _ => ""
                            },
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _workplacesBuildingCalculationService.QuoteBuildingAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.EstimateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (reply0.Feedback?.Subject?.CandidateId ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _performanceFeedbackCalculationService.EstimateFeedbackAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.NormalizeGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Building?.Manager?.Email ?? ""),
                },
                GoalId = (request.GoalId ?? ""),
                Priority = (request.Goal?.Priority ?? 0),
            },
            Outcome = new ApiPerformance.NormalizeGoalResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "NormalizeGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a goal.
    /// </summary>
    public override async Task<ApiPerformance.PreviewGoalResponse> PreviewGoal(ApiPerformance.PreviewGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.CalculateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Goal?.Assignee?.DisplayName ?? ""),
                    Number = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _documentsSignatureCalculationService.CalculateSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.QuoteFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Feedback = new ApiPerformance.Feedback
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.QuoteFeedbackAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.PreviewGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = (request.Goal?.Assignee ?? new ApiIdentity.UserRef()),
                GoalId = (request.GoalId ?? ""),
            },
            Outcome = new ApiPerformance.PreviewGoalResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "PreviewGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a goal.
    /// </summary>
    public override async Task<ApiPerformance.ScoreGoalResponse> ScoreGoal(ApiPerformance.ScoreGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiAudit.BrowseAuditEventsResponse();
        foreach (var item in (request.Goal?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.GoalComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiAudit.BrowseAuditEventsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiAudit.BrowseAuditEventsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    OwnerId = (item.CommentId ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            };
            using var rpc0 = _auditAuditEventQueryService.BrowseAuditEventsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiPerformance.CalculateFeedbackRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CalculateFeedbackRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _performanceFeedbackCalculationService.CalculateFeedbackAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ReconcileDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Goal?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Goal?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsDocumentTemplateSyncService.ReconcileDocumentTemplatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.ScoreGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.Feedback?.Subject?.Email ?? ""),
                },
                GoalId = (request.GoalId ?? ""),
                Priority = (request.Goal?.Priority ?? 0),
                Sla = new ApiPerformance.GoalSla
                {
                    DueAt = (reply2.DocumentTemplate?.DueAt ?? new Timestamp()),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ScoreGoal", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a goal.
    /// </summary>
    public override async Task<ApiPerformance.ValidateGoalResponse> ValidateGoal(ApiPerformance.ValidateGoalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateGoal for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMedia.WatchMediaAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Goal?.Assignee?.Email ?? ""),
            },
        };
        using var stream0 = _mediaMediaAssetService.WatchMediaAssets(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchMediaAssets frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiMedia.CreateMediaAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MediaAsset = new ApiMedia.MediaAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _mediaMediaAssetService.CreateMediaAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiPerformance.ExplainFeedbackCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.ExplainFeedbackCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _performanceFeedbackCalculationService.ExplainFeedbackCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.ValidateGoalResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
                Requester = (request.Goal?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Outcome = new ApiPerformance.ValidateGoalResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ValidateGoal", (request.Reason ?? ""), token);
        return response;
    }
}
