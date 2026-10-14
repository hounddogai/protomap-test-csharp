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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class SlaPolicyWorkflowEndpoint
{
    /// <summary>
    /// Advances a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.AdvanceSlaPolicyWorkflowResponse> AdvanceSlaPolicyWorkflow(ApiHelpdesk.AdvanceSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CommentEscalationApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CommentEscalationApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                    UserId = (request.SlaPolicy?.Owner?.UserId ?? ""),
                },
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (request.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskEscalationApprovalService.CommentEscalationApprovalAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiHelpdesk.ActivateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ActivateSlaPolicyRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Escalation?.Requester?.PreferredChannel ?? ""),
                },
            },
            SlaPolicyId = (request.SlaPolicy?.SlaPolicyId ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _helpdeskSlaPolicyLifecycleService.ActivateSlaPolicyAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiItsm.RetryChangeRequestWorkflowRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Escalation?.Assignee?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.SendAsync(input2, token, _itsmChangeRequestWorkflowService.RetryChangeRequestWorkflowAsync);
        var response = new ApiHelpdesk.AdvanceSlaPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Effective = (reply1.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                Name = (request.SlaPolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply2.ChangeRequest?.Assignee?.UserId ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "AdvanceSlaPolicyWorkflow", (request.SlaPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.CancelSlaPolicyWorkflowResponse> CancelSlaPolicyWorkflow(ApiHelpdesk.CancelSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.PauseDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Discount = new ApiDevices.DevicePolicyDiscountAction
                {
                    Percent = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                    {
                        ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyWorkflowService.PauseDevicePolicyWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
            {
                ApiDevices.DevicePolicy.ActionOneofCase.Block => (reply0.DevicePolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _employeesOrgUnitQueryService.SummarizeOrgUnitsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.SignalLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Tax = new ApiLicensing.LicenseAssignmentLineTax
                    {
                        Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                        {
                            ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentWorkflowService.SignalLicenseAssignmentWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.CancelSlaPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                    Etag = (reply2.LicenseAssignment?.Audit?.Etag ?? ""),
                },
                Name = (reply0.DevicePolicy?.Name ?? ""),
                Parameters = { (request.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "CancelSlaPolicyWorkflow", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.InspectSlaPolicyWorkflowResponse> InspectSlaPolicyWorkflow(ApiHelpdesk.InspectSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        _logger.LogInformation("Handling InspectSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "InspectSlaPolicyWorkflow", "accepted", token);
        var response = new ApiHelpdesk.InspectSlaPolicyWorkflowResponse
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
    public override async Task<ApiHelpdesk.ListSlaPolicyWorkflowsResponse> ListSlaPolicyWorkflows(ApiHelpdesk.ListSlaPolicyWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListSlaPolicyWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", "batch",
            "ListSlaPolicyWorkflows", "accepted", token);
        var input0 = new ApiDevices.PauseManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDevices.PauseManagedDeviceWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _devicesManagedDeviceWorkflowService.PauseManagedDeviceWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ListSlaPolicyWorkflowsResponse
        {
            SlaPolicies = { new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                    UserId = (reply0.ManagedDevice?.Owner?.UserId ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ListSlaPolicyWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.PauseSlaPolicyWorkflowResponse> PauseSlaPolicyWorkflow(ApiHelpdesk.PauseSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ReconcileProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ReconcileProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (request.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _itsmProblemSyncService.ReconcileProblemsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.SignalManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SignalManagedDeviceWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Problem?.Requester?.Email ?? ""),
                    Name = (reply0.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Problem?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Problem?.Assignee?.Email ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _devicesManagedDeviceWorkflowService.SignalManagedDeviceWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.PauseSlaPolicyWorkflowResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Problem?.Audit?.Etag ?? ""),
                },
                Description = (reply1.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                Owner = (request.SlaPolicy?.Owner ?? new ApiIdentity.UserRef()),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "PauseSlaPolicyWorkflow", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ResumeSlaPolicyWorkflowResponse> ResumeSlaPolicyWorkflow(ApiHelpdesk.ResumeSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ResumeSlaPolicyWorkflowToPreviewPosition0();
        using var rpc0 = _employeesPositionValidationService.PreviewPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.ListNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.ListNotificationsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
            },
        };
        var reply1 = await SendResumeSlaPolicyWorkflow1Async(input1, token);
        var response = new ApiHelpdesk.ResumeSlaPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Effective = (request.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ResumeSlaPolicyWorkflow", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.RetrySlaPolicyWorkflowResponse> RetrySlaPolicyWorkflow(ApiHelpdesk.RetrySlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetrySlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CancelDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.SlaPolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _devicesDevicePolicyWorkflowService.CancelDevicePolicyWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamSignatureNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Notify?.Email ?? ""),
                Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _documentsSignatureNotificationService.StreamSignatureNotices(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamSignatureNotices frame {Type}", change1.GetType().Name);
        }
        var response = new ApiHelpdesk.RetrySlaPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Description = (request.SlaPolicy?.Description ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "RetrySlaPolicyWorkflow", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.SignalSlaPolicyWorkflowResponse> SignalSlaPolicyWorkflow(ApiHelpdesk.SignalSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmployeeService.CreateEmployeeAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.SignalTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskTicketWorkflowService.SignalTicketWorkflowAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiHelpdesk.SignalSlaPolicyWorkflowResponse
        {
            Outcome = new ApiHelpdesk.SignalSlaPolicyWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Employee?.Audit?.CreatedBy ?? ""),
                },
                Name = (request.SlaPolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Employee?.Contact?.Email ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "SignalSlaPolicyWorkflow", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.StartSlaPolicyWorkflowResponse> StartSlaPolicyWorkflow(ApiHelpdesk.StartSlaPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartSlaPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.SlaPolicy?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiHelpdesk.SlaPolicyCondition>()).Select(async item =>
        {
            var input0 = new ApiLicensing.SubmitEntitlementRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Entitlement = new ApiLicensing.Entitlement
                {
                    Conditions = { new ApiLicensing.EntitlementCondition
                    {
                        Value = (item.Value ?? ""),
                    } },
                },
            };
            using var rpc0 = _licensingEntitlementApprovalService.SubmitEntitlementAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiLicensing.SubmitEntitlementResponse();
        var input1 = new ApiHelpdesk.ValidateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _helpdeskKnowledgeArticleValidationService.ValidateKnowledgeArticleAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.StartSlaPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.KnowledgeArticle?.Audit?.CreatedBy ?? ""),
                },
                Name = (request.SlaPolicy?.Name ?? ""),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "StartSlaPolicyWorkflow", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    private async Task<ApiNotifications.ListNotificationsResponse> SendResumeSlaPolicyWorkflow1Async(ApiNotifications.ListNotificationsRequest request, CancellationToken token)
    {
        return await _slaPolicyWorkflowEndpointSendResumeSlaPolicyWorkflow1Port.SendAsync(request, token);
    }
}
