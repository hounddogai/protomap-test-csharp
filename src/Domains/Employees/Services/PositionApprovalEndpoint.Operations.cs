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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiSubscriptions = Acme.Subscriptions.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

public sealed partial class PositionApprovalEndpoint
{
    /// <summary>
    /// Approves a position.
    /// </summary>
    public override async Task<ApiEmployees.ApprovePositionResponse> ApprovePosition(ApiEmployees.ApprovePositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApprovePosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.RecallOrgUnitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.Position?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Details?.Approver?.UserId ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.Position?.Owner?.DisplayName ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _sendApprovePosition0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ApprovePositionResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.OrgUnit?.Audit?.Etag ?? ""),
                },
                Description = (request.Position?.Description ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ApprovePosition", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a position.
    /// </summary>
    public override async Task<ApiEmployees.CommentPositionApprovalResponse> CommentPositionApproval(ApiEmployees.CommentPositionApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentPositionApproval for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiOnboarding.GenerateEquipmentReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (request.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _onboardingEquipmentReportingService.GenerateEquipmentReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ResumeEmployeeWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ResumeEmployeeWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Employee = new ApiEmployees.Employee
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiEmployees.ResumeEmployeeWorkflowRequest, ApiEmployees.ResumeEmployeeWorkflowResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeWorkflowService", "ResumeEmployeeWorkflow",
            Wire.Marshaller<ApiEmployees.ResumeEmployeeWorkflowRequest>(), Wire.Marshaller<ApiEmployees.ResumeEmployeeWorkflowResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.CommentPositionApprovalResponse
        {
            Outcome = new ApiEmployees.CommentPositionApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Position = new ApiEmployees.Position
            {
                Name = (request.Position?.Name ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "CommentPositionApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a position.
    /// </summary>
    public override async Task<ApiEmployees.DelegatePositionApprovalResponse> DelegatePositionApproval(ApiEmployees.DelegatePositionApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegatePositionApproval for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NormalizeDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Position?.Audit?.CreatedBy ?? ""),
                            UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Position?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var command0 = new DispatchCommand<ApiDocuments.NormalizeDocumentFolderRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _documentsDocumentFolderValidationService.NormalizeDocumentFolderAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.DelegatePositionApprovalResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Description = (request.Position?.Description ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "DelegatePositionApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a position.
    /// </summary>
    public override async Task<ApiEmployees.EscalatePositionResponse> EscalatePosition(ApiEmployees.EscalatePositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalatePosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckEmploymentContractEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.Position?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Tax = new ApiEmployees.EmploymentContractLineTax
                    {
                        Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _employeesEmploymentContractValidationService.CheckEmploymentContractEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.GetPositionRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            PositionId = (request.PositionId ?? ""),
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _employeesPositionValidationService.GetPositionRulesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.EscalatePositionResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = (reply1.Position?.Owner ?? new ApiIdentity.UserRef()),
                PositionId = (request.PositionId ?? ""),
                Status = (request.Position?.Status ?? (ApiEmployees.PositionStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "EscalatePosition", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a position.
    /// </summary>
    public override async Task<ApiEmployees.GetPositionApprovalStatusResponse> GetPositionApprovalStatus(ApiEmployees.GetPositionApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PositionId, "position_id");
        _logger.LogInformation("Handling GetPositionApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "position", request.PositionId,
            "GetPositionApprovalStatus", "accepted", token);
        var input0 = new ApiDocuments.ResolveDocumentConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                DueAt = (request.Context?.Deadline ?? new Timestamp()),
                Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.ResolveDocumentConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.GetPositionApprovalStatusResponse
        {
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Document?.Audit?.UpdatedBy ?? ""),
                },
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists positions a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListPendingPositionsResponse> ListPendingPositions(ApiEmployees.ListPendingPositionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingPositions for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "position", "batch",
            "ListPendingPositions", "accepted", token);
        var response = new ApiEmployees.ListPendingPositionsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ListPendingPositions", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a position.
    /// </summary>
    public override async Task<ApiEmployees.RejectPositionResponse> RejectPosition(ApiEmployees.RejectPositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectPosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.DelegateKnowledgeArticleApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendRejectPosition0Async(input0, token);
        var input1 = new ApiDocuments.SendDocumentTemplateTestNoticeRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTemplateTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
                Variables = { (reply0.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (request.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.SendDocumentTemplateTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSubscriptions.SearchSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSubscriptions.SearchSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Position?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply1.DocumentTemplate?.BillingAddress?.Region ?? ""),
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiSubscriptions.SearchSubscriptionsRequest>();
        using var rpc2 = _subscriptionsSubscriptionService.SearchSubscriptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.RejectPositionResponse
        {
            Outcome = new ApiEmployees.RejectPositionResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Name = (request.Position?.Name ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "RejectPosition", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a position.
    /// </summary>
    public override async Task<ApiEmployees.SubmitPositionResponse> SubmitPosition(ApiEmployees.SubmitPositionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitPosition for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.SubmitVisitorPassRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Position?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Position?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.Position?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiWorkplaces.SubmitVisitorPassRequest>(json0);
        using var rpc0 = _workplacesVisitorPassApprovalService.SubmitVisitorPassAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiVenues.SearchVenuesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiVenues.SearchVenuesRequest.Types.Filter
        {
            CreatedAfter = (reply0.VisitorPass?.IssuedAt ?? new Timestamp()),
            Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            OwnerId = (reply0.Result?.CorrelationId ?? ""),
            Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
        };
        using var rpc1 = _venuesVenueService.SearchVenuesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.SubmitPositionResponse
        {
            Position = new ApiEmployees.Position
            {
                Description = (request.Position?.Description ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "SubmitPosition", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiHelpdesk.DelegateKnowledgeArticleApprovalResponse> SendRejectPosition0Async(ApiHelpdesk.DelegateKnowledgeArticleApprovalRequest request, CancellationToken token)
    {
        return await _positionApprovalEndpointSendRejectPosition0Port.SendAsync(request, token);
    }
}
