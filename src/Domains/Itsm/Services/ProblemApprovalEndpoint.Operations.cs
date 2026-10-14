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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ProblemApprovalEndpoint
{
    /// <summary>
    /// Approves a problem.
    /// </summary>
    public override async Task<ApiItsm.ApproveProblemResponse> ApproveProblem(ApiItsm.ApproveProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.ResolveUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiIdentity.ResolveUsersRequest, ApiIdentity.ResolveUsersResponse>(
            MethodType.Unary, "acme.identity.v1" + ".UserQueryService", "ResolveUsers",
            Wire.Marshaller<ApiIdentity.ResolveUsersRequest>(), Wire.Marshaller<ApiIdentity.ResolveUsersResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ListComplianceScanWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListComplianceScanWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Problem?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        var command1 = new DispatchCommand<ApiDevices.ListComplianceScanWorkflowsRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _devicesComplianceScanWorkflowService.ListComplianceScanWorkflowsAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.FindUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.FindUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Problem?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                {
                    ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.EmployeeId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _identityUserQueryService.FindUsersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.ReinstateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ReinstateProblemRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Problem?.Assignee?.Email ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.Problem?.Requester?.Email ?? ""),
                },
            },
            ProblemId = (request.Problem?.ProblemId ?? ""),
        };
        using var rpc3 = _itsmProblemLifecycleService.ReinstateProblemAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiItsm.ReinstateProblemRequest>("acme.itsm.v1.ProblemApprovalService/ApproveProblem/3", input3.Clone()), token);
        var response = new ApiItsm.ApproveProblemResponse
        {
            Outcome = new ApiItsm.ApproveProblemResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.Problem?.Assignee?.DisplayName ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ApproveProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a problem.
    /// </summary>
    public override async Task<ApiItsm.CommentProblemApprovalResponse> CommentProblemApproval(ApiItsm.CommentProblemApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentProblemApproval for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.CommentProblemApprovalToRecallIncident0();
        using var rpc0 = _itsmIncidentApprovalService.RecallIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.CommentProblemApprovalResponse
        {
            Problem = new ApiItsm.Problem
            {
                Priority = (request.Problem?.Priority ?? 0),
                ProblemId = (request.ProblemId ?? ""),
                Requester = (reply0.Incident?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "CommentProblemApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a problem.
    /// </summary>
    public override async Task<ApiItsm.DelegateProblemApprovalResponse> DelegateProblemApproval(ApiItsm.DelegateProblemApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateProblemApproval for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecallDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecallDevicePolicyRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Problem?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Problem?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyApprovalService.RecallDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ShareProblemReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ShareProblemReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Phone = (request.Problem?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _itsmProblemReportingService.ShareProblemReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.DelegateProblemApprovalResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "DelegateProblemApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a problem.
    /// </summary>
    public override async Task<ApiItsm.GetProblemApprovalStatusResponse> GetProblemApprovalStatus(ApiItsm.GetProblemApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        _logger.LogInformation("Handling GetProblemApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "GetProblemApprovalStatus", "accepted", token);
        var response = new ApiItsm.GetProblemApprovalStatusResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists problems a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListPendingProblemsResponse> ListPendingProblems(ApiItsm.ListPendingProblemsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingProblems for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", "batch",
            "ListPendingProblems", "accepted", token);
        var input0 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ScoreUserRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiIdentity.ScoreUserRequest>();
        using var rpc0 = _identityUserValidationService.ScoreUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ListPendingProblemsResponse
        {
            Problems = { new ApiItsm.Problem
            {
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.User?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ListPendingProblems", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a problem.
    /// </summary>
    public override async Task<ApiItsm.RecallProblemResponse> RecallProblem(ApiItsm.RecallProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()).Select(async item =>
        {
            var input0 = new ApiDocuments.CheckSignatureEligibilityRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.CheckSignatureEligibilityRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (item.AuthorEmail ?? ""),
                        Phone = (request.Problem?.Requester?.Phone ?? ""),
                    },
                },
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiDocuments.SignatureMeasurement
                    {
                        ObservedAt = (item.CreatedAt ?? new Timestamp()),
                    } },
                    Subject = new ApiDocuments.DocumentRef
                    {
                        DisplayName = (request.Problem?.Customer?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc0 = _documentsSignatureValidationService.CheckSignatureEligibilityAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.CheckSignatureEligibilityResponse();
        var response = new ApiItsm.RecallProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "RecallProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a problem.
    /// </summary>
    public override async Task<ApiItsm.RejectProblemResponse> RejectProblem(ApiItsm.RejectProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.RejectServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Problem?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Problem?.Requester?.Email ?? ""),
                    UserId = (request.Problem?.Assignee?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _itsmServiceAssetApprovalService.RejectServiceAssetAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiIdentity.SuggestUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.SuggestUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Problem?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _identityUserQueryService.SuggestUsersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var batch2 = (request.Problem?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemStatusChange>()).Select(async item =>
        {
            var input2 = new ApiItsm.RecalculateIncidentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Incident = new ApiItsm.Incident
                {
                    History = { new ApiItsm.IncidentStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                    } },
                },
            };
            using var rpc2 = _itsmIncidentCalculationService.RecalculateIncidentsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiItsm.RecalculateIncidentsResponse();
        var input3 = new ApiItsm.CommentChangeRequestApprovalRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Problem?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmChangeRequestApprovalService.CommentChangeRequestApprovalAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.RejectProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                Priority = (request.Problem?.Priority ?? 0),
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply3.ChangeRequest?.Requester?.Email ?? ""),
                },
                Tags = { (reply0.ServiceAsset?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "RejectProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a problem.
    /// </summary>
    public override async Task<ApiItsm.SubmitProblemResponse> SubmitProblem(ApiItsm.SubmitProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SubmitIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SubmitIncidentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Problem?.Assignee?.DisplayName ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Channel = new ApiItsm.IncidentChannelOrigin
                {
                    Address = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Channel => (request.Problem?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _itsmIncidentApprovalService.SubmitIncidentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CheckPositionEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Incident?.Requester?.Phone ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Incident?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.Incident?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc1 = _sendSubmitProblem1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.SubmitProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Incident?.Audit?.CreatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Requester = (request.Problem?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "SubmitProblem", (request.Reason ?? ""), token);
        return response;
    }
}
