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
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class SlaPolicyApprovalEndpoint
{
    /// <summary>
    /// Approves a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ApproveSlaPolicyResponse> ApproveSlaPolicy(ApiHelpdesk.ApproveSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SignalIncidentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmIncidentWorkflowService.SignalIncidentWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiMaintenance.CompareWorkOrderOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            WorkOrder = new ApiMaintenance.WorkOrder
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Channel = new ApiMaintenance.WorkOrderChannelOrigin
                {
                    Address = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Channel => (reply0.Incident?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _maintenanceWorkOrderCalculationService.CompareWorkOrderOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ApproveSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.ApproveSlaPolicyResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Incident?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.WorkOrder?.Assignee?.DisplayName ?? ""),
                    Email = (reply1.WorkOrder?.Customer?.Email ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ApproveSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.CommentSlaPolicyApprovalResponse> CommentSlaPolicyApproval(ApiHelpdesk.CommentSlaPolicyApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentSlaPolicyApproval for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.RejectTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.RejectTicketRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _sendCommentSlaPolicyApproval0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.SlaPolicy?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.SlaPolicyCondition>()).Select(async item =>
        {
            var input1 = new ApiAuthorization.CheckAccessPolicyEligibilityRequest
            {
                AccessPolicy = new ApiAuthorization.AccessPolicy
                {
                    Conditions = { new ApiAuthorization.AccessPolicyCondition
                    {
                        Attribute = (item.Attribute ?? ""),
                        Negate = item.Negate,
                        Operator = (item.Operator ?? ""),
                        Value = (item.Value ?? ""),
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc1 = _authorizationAccessPolicyValidationService.CheckAccessPolicyEligibilityAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiAuthorization.CheckAccessPolicyEligibilityResponse();
        var input2 = new ApiHelpdesk.ScheduleEscalationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply1.AccessPolicy ?? new ApiAuthorization.AccessPolicy()).ActionCase switch
            {
                ApiAuthorization.AccessPolicy.ActionOneofCase.Block => (reply1.AccessPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc2 = _helpdeskEscalationReportingService.ScheduleEscalationReportAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiHelpdesk.ScheduleEscalationReportRequest>("acme.helpdesk.v1.SlaPolicyApprovalService/CommentSlaPolicyApproval/2", input2.Clone()), token);
        var response = new ApiHelpdesk.CommentSlaPolicyApprovalResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
                Version = (request.SlaPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "CommentSlaPolicyApproval", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.DelegateSlaPolicyApprovalResponse> DelegateSlaPolicyApproval(ApiHelpdesk.DelegateSlaPolicyApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateSlaPolicyApproval for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainSlaPolicyDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainSlaPolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
                Route = new ApiHelpdesk.SlaPolicyRouteAction
                {
                    Weight = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                    {
                        ApiHelpdesk.SlaPolicy.ActionOneofCase.Route => (request.SlaPolicy?.Route?.Weight ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc0 = _helpdeskSlaPolicyValidationService.ExplainSlaPolicyDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ListTicketWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListTicketWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (reply0.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _helpdeskTicketWorkflowService.ListTicketWorkflowsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiHelpdesk.DelegateSlaPolicyApprovalResponse
        {
            Outcome = new ApiHelpdesk.DelegateSlaPolicyApprovalResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Owner = (request.SlaPolicy?.Owner ?? new ApiIdentity.UserRef()),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "DelegateSlaPolicyApproval", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.EscalateSlaPolicyResponse> EscalateSlaPolicy(ApiHelpdesk.EscalateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.EscalateSlaPolicyToCompareWorkOrderOptions0();
        using var rpc0 = _maintenanceWorkOrderCalculationService.CompareWorkOrderOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ShareEscalationReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ShareEscalationReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.WorkOrder ?? new ApiMaintenance.WorkOrder()).OriginCase switch
                    {
                        ApiMaintenance.WorkOrder.OriginOneofCase.Customer => (reply0.WorkOrder?.Customer?.Email ?? ""),
                        _ => ""
                    },
                    Phone = (reply0.WorkOrder?.Requester?.Phone ?? ""),
                    PreferredChannel = (reply0.WorkOrder?.Requester?.PreferredChannel ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _helpdeskEscalationReportingService.ShareEscalationReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.SubmitComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _devicesComplianceScanApprovalService.SubmitComplianceScanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.ScheduleEscalationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ScheduleEscalationReportRequest.Types.Details
            {
                Period = (request.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            EscalationId = (reply1.Escalation?.EscalationId ?? ""),
            Reason = (request.Reason ?? ""),
        };
        var packed3 = Any.Pack(input3);
        input3 = packed3.Unpack<ApiHelpdesk.ScheduleEscalationReportRequest>();
        using var rpc3 = _helpdeskEscalationReportingService.ScheduleEscalationReportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.EscalateSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.EscalateSlaPolicyResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.ComplianceScan?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.WorkOrder?.Assignee?.DisplayName ?? ""),
                },
                Priority = (request.SlaPolicy?.Priority ?? 0),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "EscalateSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.GetSlaPolicyApprovalStatusResponse> GetSlaPolicyApprovalStatus(ApiHelpdesk.GetSlaPolicyApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        _logger.LogInformation("Handling GetSlaPolicyApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "GetSlaPolicyApprovalStatus", "accepted", token);
        var response = new ApiHelpdesk.GetSlaPolicyApprovalStatusResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists sla policies a page at a time.
    /// </summary>
    public override async Task<ApiHelpdesk.ListPendingSlaPoliciesResponse> ListPendingSlaPolicies(ApiHelpdesk.ListPendingSlaPoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingSlaPolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", "batch",
            "ListPendingSlaPolicies", "accepted", token);
        var input0 = new ApiEmployees.ExplainEmploymentContractDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.ExplainEmploymentContractDecisionRequest>(json0);
        using var rpc0 = _employeesEmploymentContractValidationService.ExplainEmploymentContractDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ListPendingSlaPoliciesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            SlaPolicies = { new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ListPendingSlaPolicies", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.RecallSlaPolicyResponse> RecallSlaPolicy(ApiHelpdesk.RecallSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ListTicketWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiHelpdesk.ListTicketWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _helpdeskTicketWorkflowService.ListTicketWorkflowsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.ScoreEscalationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
                Customer = new ApiHelpdesk.TicketRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskEscalationValidationService.ScoreEscalationAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDocuments.LintDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.LintDocumentsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentValidationService.LintDocumentsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiHelpdesk.EscalateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EscalateTicketRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Escalation ?? new ApiHelpdesk.Escalation()).OriginCase switch
                    {
                        ApiHelpdesk.Escalation.OriginOneofCase.Customer => (reply1.Escalation?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply1.Escalation?.Requester?.Email ?? ""),
                },
            },
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Escalation?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc3 = _helpdeskTicketApprovalService.EscalateTicketAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.RecallSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.RecallSlaPolicyResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Document?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply3.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Escalation?.Requester?.Email ?? ""),
                },
                Parameters = { (request.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Priority = (reply1.Escalation?.Customer?.Priority ?? 0),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "RecallSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.RejectSlaPolicyResponse> RejectSlaPolicy(ApiHelpdesk.RejectSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SignalManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SignalManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SlaPolicy?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _devicesManagedDeviceWorkflowService.SignalManagedDeviceWorkflowAsync);
        var response = new ApiHelpdesk.RejectSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.RejectSlaPolicyResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Name = (request.SlaPolicy?.Name ?? ""),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "RejectSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.SubmitSlaPolicyResponse> SubmitSlaPolicy(ApiHelpdesk.SubmitSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ScheduleKnowledgeArticleExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                    {
                        ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleLifecycleService.ScheduleKnowledgeArticleExpiryAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ApproveComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ApproveComplianceScanRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                    Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanApprovalService.ApproveComplianceScanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.SubmitSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.SubmitSlaPolicyResponse.Types.Outcome
            {
                NextSteps = { (reply1.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
                Version = (request.SlaPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "SubmitSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
}
