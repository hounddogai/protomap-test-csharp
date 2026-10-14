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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

public sealed partial class PositionWorkflowEndpoint
{
    /// <summary>
    /// Advances a position.
    /// </summary>
    public override async Task<ApiEmployees.AdvancePositionWorkflowResponse> AdvancePositionWorkflow(ApiEmployees.AdvancePositionWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvancePositionWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Position?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Position?.Owner?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Description = (request.Position?.Description ?? ""),
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
        using var rpc0 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.AdvancePositionWorkflowResponse
        {
            Outcome = new ApiEmployees.AdvancePositionWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Position = new ApiEmployees.Position
            {
                Parameters = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "AdvancePositionWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists positions a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListPositionWorkflowsResponse> ListPositionWorkflows(ApiEmployees.ListPositionWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPositionWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "position", "batch",
            "ListPositionWorkflows", "accepted", token);
        var response = new ApiEmployees.ListPositionWorkflowsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ListPositionWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a position.
    /// </summary>
    public override async Task<ApiEmployees.ResumePositionWorkflowResponse> ResumePositionWorkflow(ApiEmployees.ResumePositionWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumePositionWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Position?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.PositionCondition>()).Select(async item =>
        {
            var input0 = new ApiAuthorization.NormalizeAccessPolicyRequest
            {
                AccessPolicy = new ApiAuthorization.AccessPolicy
                {
                    Conditions = { new ApiAuthorization.AccessPolicyCondition
                    {
                        Negate = item.Negate,
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _authorizationAccessPolicyValidationService.NormalizeAccessPolicyAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiAuthorization.NormalizeAccessPolicyResponse();
        var reply1 = new ApiTimekeeping.DelegateOvertimeApprovalResponse();
        foreach (var item in (request.Position?.Conditions ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.PositionCondition>()))
        {
            var input1 = new ApiTimekeeping.DelegateOvertimeApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Overtime = new ApiTimekeeping.Overtime
                {
                    Conditions = { new ApiTimekeeping.OvertimeCondition
                    {
                        Negate = item.Negate,
                    } },
                },
            };
            using var rpc1 = _timekeepingOvertimeApprovalService.DelegateOvertimeApprovalAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiCorporateevents.GenerateRegistrationReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
            {
                ApiEmployees.Position.ActionOneofCase.Block => (request.Position?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc2 = _corporateeventsRegistrationReportingService.GenerateRegistrationReportAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.ResumePositionWorkflowResponse
        {
            Position = new ApiEmployees.Position
            {
                Name = (request.Position?.Name ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "ResumePositionWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a position.
    /// </summary>
    public override async Task<ApiEmployees.RetryPositionWorkflowResponse> RetryPositionWorkflow(ApiEmployees.RetryPositionWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryPositionWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ApproveOrgUnitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (request.Position?.Audit?.CreatedBy ?? ""),
                            UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                            UpdatedBy = (request.Position?.Audit?.UpdatedBy ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Position?.Owner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _sendRetryPositionWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.RetryPositionWorkflowResponse
        {
            Outcome = new ApiEmployees.RetryPositionWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Name = (request.Position?.Name ?? ""),
                PositionId = (request.PositionId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "RetryPositionWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a position.
    /// </summary>
    public override async Task<ApiEmployees.SignalPositionWorkflowResponse> SignalPositionWorkflow(ApiEmployees.SignalPositionWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalPositionWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SubmitPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Block = new ApiEmployees.PositionBlockAction
                {
                    NotifyEmail = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Block => (request.Position?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Position?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _employeesPositionApprovalService.SubmitPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExplainPositionDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Discount = new ApiEmployees.PositionDiscountAction
                {
                    Percent = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                    UserId = (reply0.Position?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _employeesPositionValidationService.ExplainPositionDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.SignalPositionWorkflowResponse
        {
            Outcome = new ApiEmployees.SignalPositionWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Position = new ApiEmployees.Position
            {
                Effective = (reply1.Position?.Effective ?? new ApiCommon.TimeWindow()),
                Parameters = { (request.Position?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                PositionId = (reply0.Position?.PositionId ?? ""),
                Priority = (request.Position?.Priority ?? 0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "SignalPositionWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a position.
    /// </summary>
    public override async Task<ApiEmployees.StartPositionWorkflowResponse> StartPositionWorkflow(ApiEmployees.StartPositionWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartPositionWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Position is { } record)
        {
            var snapshot = PositionMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CountEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.CountEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Position?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Position?.Owner?.UserId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.CountEmployeesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmployeeQueryService.CountEmployeesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.UpdateAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Position?.Audit?.Etag ?? ""),
                },
                Discount = new ApiAuthorization.AccessPolicyDiscountAction
                {
                    Amount = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                    Percent = (request.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Discount => (request.Position?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Position?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _authorizationAccessPolicyService.UpdateAccessPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.RecallOrgUnitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Position?.Owner?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _employeesOrgUnitApprovalService.RecallOrgUnitAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.StartPositionWorkflowResponse
        {
            Outcome = new ApiEmployees.StartPositionWorkflowResponse.Types.Outcome
            {
                Message = (reply2.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.AccessPolicy?.Audit?.Etag ?? ""),
                },
                Owner = (request.Position?.Owner ?? new ApiIdentity.UserRef()),
                PositionId = (request.PositionId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "position",
            "audit", "StartPositionWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
