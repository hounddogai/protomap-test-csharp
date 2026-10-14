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
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiTreasury = Acme.Treasury.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class EscalationLifecycleEndpoint
{
    /// <summary>
    /// Activates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ActivateEscalationResponse> ActivateEscalation(ApiHelpdesk.ActivateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling ActivateEscalation for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "ActivateEscalation", "accepted", token);
        var input0 = new ApiItsm.PushProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PushProblemRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _itsmProblemSyncService.PushProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.NormalizePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.NormalizePositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Problem?.Requester?.Phone ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (reply0.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (reply0.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (reply0.Problem?.Customer?.Email ?? ""),
                        _ => ""
                    },
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _employeesPositionValidationService.NormalizePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ActivateEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Problem?.Requester?.Phone ?? ""),
                },
            },
            Outcome = new ApiHelpdesk.ActivateEscalationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ActivateEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ExpireEscalationResponse> ExpireEscalation(ApiHelpdesk.ExpireEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling ExpireEscalation for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "ExpireEscalation", "accepted", token);
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.ExpireEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.ExpireEscalationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ExpireEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.RenewEscalationResponse> RenewEscalation(ApiHelpdesk.RenewEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling RenewEscalation for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "RenewEscalation", "accepted", token);
        var input0 = new ApiHelpdesk.GenerateEscalationReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _helpdeskEscalationReportingService.GenerateEscalationReportAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiEmployees.UpdateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmployeeService.UpdateEmployeeAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Employee?.Contact?.Email ?? ""),
                },
            },
        };
        var named2 = _registry.Get<ApiEmployees.EmployeeService.EmployeeServiceClient>("acme.employees.v1.EmployeeService");
        using var rpc2 = named2.CreateEmployeeAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.CreateEmployeeRequest>("acme.helpdesk.v1.EscalationLifecycleService/RenewEscalation/2", input2.Clone()), token);
        var input3 = new ApiAudit.CheckAuditEventEligibilityRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    Number = (reply1.Employee ?? new ApiEmployees.Employee()).IdentityDocumentCase switch
                    {
                        ApiEmployees.Employee.IdentityDocumentOneofCase.NationalId => (reply1.Employee?.NationalId?.Number ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.CheckAuditEventEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply2.Employee?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc3 = _auditAuditEventValidationService.CheckAuditEventEligibilityAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.RenewEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (reply1.Employee?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.RenewEscalationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "RenewEscalation", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.ScheduleEscalationExpiryResponse> ScheduleEscalationExpiry(ApiHelpdesk.ScheduleEscalationExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleEscalationExpiry for {Tenant}", requestContext.TenantId);
        if (request.Escalation is { } record)
        {
            var snapshot = EscalationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Escalation?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.Escalation?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.AdvanceLicenseAssignmentWorkflowRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingLicenseAssignmentWorkflowService.AdvanceLicenseAssignmentWorkflowAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTreasury.UpdateCashPositionRequest
        {
            CashPosition = new ApiTreasury.CashPosition
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                },
                Bank = new ApiTreasury.CashPositionBankFunding
                {
                    Iban = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (reply0.LicenseAssignment?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _treasuryCashPositionService.UpdateCashPositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.ScheduleEscalationExpiryResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EscalationId = (request.EscalationId ?? ""),
                Requester = (request.Escalation?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Outcome = new ApiHelpdesk.ScheduleEscalationExpiryResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "ScheduleEscalationExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.SuspendEscalationResponse> SuspendEscalation(ApiHelpdesk.SuspendEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling SuspendEscalation for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "SuspendEscalation", "accepted", token);
        var input0 = new ApiLicensing.EscalateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingSoftwareLicenseApprovalService.EscalateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SuspendEscalationToQuoteSlaPolicy1(reply0);
        using var rpc1 = _helpdeskSlaPolicyCalculationService.QuoteSlaPolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.SuspendEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                EscalationId = (request.EscalationId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (reply0.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
            Outcome = new ApiHelpdesk.SuspendEscalationResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "SuspendEscalation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an escalation.
    /// </summary>
    public override async Task<ApiHelpdesk.TerminateEscalationResponse> TerminateEscalation(ApiHelpdesk.TerminateEscalationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EscalationId, "escalation_id");
        _logger.LogInformation("Handling TerminateEscalation for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "escalation", request.EscalationId,
            "TerminateEscalation", "accepted", token);
        var input0 = new ApiLicensing.StartSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.StartSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply0 = await SendTerminateEscalation0Async(input0, token);
        var input1 = new ApiEmployees.BatchGetEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeesEmployeeService.BatchGetEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTreasury.CreateCashPositionRequest
        {
            CashPosition = new ApiTreasury.CashPosition
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                Bank = new ApiTreasury.CashPositionBankFunding
                {
                    AccountHolder = (reply0.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply0.SoftwareLicense?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiTreasury.FxHedgeRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _treasuryCashPositionService.CreateCashPositionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.TerminateEscalationResponse
        {
            Escalation = new ApiHelpdesk.Escalation
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.CashPosition?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Comments = { new ApiHelpdesk.EscalationComment
                {
                    CreatedAt = (reply2.CashPosition?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                EscalationId = (request.EscalationId ?? ""),
            },
            Outcome = new ApiHelpdesk.TerminateEscalationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "escalation",
            "audit", "TerminateEscalation", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.StartSoftwareLicenseWorkflowResponse> SendTerminateEscalation0Async(ApiLicensing.StartSoftwareLicenseWorkflowRequest request, CancellationToken token)
    {
        return await _escalationLifecycleEndpointSendTerminateEscalation0Port.SendAsync(request, token);
    }
}
