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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

public sealed partial class EmploymentContractValidationEndpoint
{
    /// <summary>
    /// Checks an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.CheckEmploymentContractEligibilityResponse> CheckEmploymentContractEligibility(ApiEmployees.CheckEmploymentContractEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckEmploymentContractEligibility for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.ListAccessPoliciesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiAuthorization.ListAccessPoliciesRequest.Types.Filter
        {
            CreatedAfter = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
            Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
        };
        using var rpc0 = _authorizationAccessPolicyService.ListAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.CheckEmploymentContractEligibilityResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = (request.EmploymentContract?.BillingAddress ?? new ApiCommon.PostalAddress()),
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "CheckEmploymentContractEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.ExplainEmploymentContractDecisionResponse> ExplainEmploymentContractDecision(ApiEmployees.ExplainEmploymentContractDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainEmploymentContractDecision for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CheckPositionEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Position?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Discount = new ApiEmployees.PositionDiscountAction
                {
                    Percent = (reply0.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Discount => (reply0.Position?.Discount?.Percent ?? 0d),
                        _ => 0d
                    },
                },
            },
        };
        using var rpc1 = _employeesPositionValidationService.ScorePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiTimekeeping.ValidateOvertimeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Overtime = new ApiTimekeeping.Overtime
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                },
                Block = new ApiTimekeeping.OvertimeBlockAction
                {
                    NotifyEmail = (reply1.Position ?? new ApiEmployees.Position()).ActionCase switch
                    {
                        ApiEmployees.Position.ActionOneofCase.Block => (reply1.Position?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _timekeepingOvertimeValidationService.ValidateOvertimeAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.ExplainEmploymentContractDecisionResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = (request.EmploymentContract?.BillingAddress ?? new ApiCommon.PostalAddress()),
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Amount = (reply1.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                } },
            },
            Outcome = new ApiEmployees.ExplainEmploymentContractDecisionResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "ExplainEmploymentContractDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.GetEmploymentContractRulesResponse> GetEmploymentContractRules(ApiEmployees.GetEmploymentContractRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmploymentContractId, "employment_contract_id");
        _logger.LogInformation("Handling GetEmploymentContractRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract", request.EmploymentContractId,
            "GetEmploymentContractRules", "accepted", token);
        var input0 = new ApiWorkplaces.ExplainBuildingDecisionRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (request.Context?.Actor?.Email ?? ""),
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ExplainBuildingDecisionRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _workplacesBuildingValidationService.ExplainBuildingDecisionAsync);
        var response = new ApiEmployees.GetEmploymentContractRulesResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Building?.Location?.Locality ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.LintEmploymentContractsResponse> LintEmploymentContracts(ApiEmployees.LintEmploymentContractsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintEmploymentContracts for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _sendLintEmploymentContracts0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAudit.ResolveAuditEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named1 = _registry.Get<ApiAudit.AuditEventQueryService.AuditEventQueryServiceClient>("acme.audit.v1.AuditEventQueryService");
        using var rpc1 = named1.ResolveAuditEventsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.SimulateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                    Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        var descriptor2 = new Method<ApiCorporateevents.SimulateCorporateEventRequest, ApiCorporateevents.SimulateCorporateEventResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".CorporateEventCalculationService", "SimulateCorporateEvent",
            Wire.Marshaller<ApiCorporateevents.SimulateCorporateEventRequest>(), Wire.Marshaller<ApiCorporateevents.SimulateCorporateEventResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLedger.ResolveLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command3 = new DispatchCommand<ApiLedger.ResolveLedgerAccountsRequest>(input3,
            "primary", requestContext.TenantId);
        var routed3 = command3 with { Region = _settings.Value.Region };
        using var rpc3 = _ledgerLedgerAccountQueryService.ResolveLedgerAccountsAsync(routed3.Payload, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.LintEmploymentContractsResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Position?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.CorporateEvent?.Location?.Recipient ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Status = (request.EmploymentContract?.Status ?? (ApiEmployees.EmploymentContractStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "LintEmploymentContracts", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.NormalizeEmploymentContractResponse> NormalizeEmploymentContract(ApiEmployees.NormalizeEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiEmployees.NormalizeEmploymentContractRequest, ApiTimekeeping.BrowseTimesheetsRequest>(value => new ApiTimekeeping.BrowseTimesheetsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiTimekeeping.BrowseTimesheetsRequest.Types.Filter
            {
                Region = (value.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _timekeepingTimesheetQueryService.BrowseTimesheetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _employeesPositionValidationService.LintPositionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.NormalizeEmploymentContractToBrowseLedgerAccounts2(reply1);
        using var rpc2 = _ledgerLedgerAccountQueryService.BrowseLedgerAccountsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Position?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Position?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Position?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply3 = await SendNormalizeEmploymentContract3Async(input3, token);
        var response = new ApiEmployees.NormalizeEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Notes = (request.EmploymentContract?.Notes ?? ""),
            },
            Outcome = new ApiEmployees.NormalizeEmploymentContractResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "NormalizeEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.PreviewEmploymentContractResponse> PreviewEmploymentContract(ApiEmployees.PreviewEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.StreamDocumentFolderNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentFolderNoticesRequest.Types.Filter
            {
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _documentsDocumentFolderNotificationService.StreamDocumentFolderNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentFolderNotices frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiEmployees.PreviewPositionRequest>();
        using var rpc1 = _employeesPositionValidationService.PreviewPositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.PreviewEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Status = (request.EmploymentContract?.Status ?? (ApiEmployees.EmploymentContractStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "PreviewEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.ScoreEmploymentContractResponse> ScoreEmploymentContract(ApiEmployees.ScoreEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.GetEmploymentContractRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContractId = (request.EmploymentContract?.EmploymentContractId ?? ""),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.GetEmploymentContractRatesRequest>(json0);
        using var rpc0 = _employeesEmploymentContractCalculationService.GetEmploymentContractRatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ScoreEmploymentContractResponse
        {
            Breakdown = new ApiEmployees.ScoreEmploymentContractResponse.Types.Breakdown
            {
                Total = (request.EmploymentContract?.Total ?? new ApiCommon.Money()),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "ScoreEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.ValidateEmploymentContractResponse> ValidateEmploymentContract(ApiEmployees.ValidateEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CheckPositionEligibilityRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiEmployees.CheckPositionEligibilityRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
        };
        input0.Position = new ApiEmployees.Position
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
            },
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.ExplainAvailabilityWindowCalculationRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (request.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Contact?.Email ?? ""),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _schedulingAvailabilityWindowCalculationService.ExplainAvailabilityWindowCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.ValidateEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.AvailabilityWindow?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Position?.Audit?.UpdatedBy ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                IssuedAt = (request.EmploymentContract?.IssuedAt ?? new Timestamp()),
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Description = (reply0.Position?.Description ?? ""),
                } },
            },
            Outcome = new ApiEmployees.ValidateEmploymentContractResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "ValidateEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.ValidatePositionResponse> SendNormalizeEmploymentContract3Async(ApiEmployees.ValidatePositionRequest request, CancellationToken token)
    {
        return await _employmentContractValidationEndpointSendNormalizeEmploymentContract3Port.SendAsync(request, token);
    }
}
