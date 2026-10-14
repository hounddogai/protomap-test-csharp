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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiPromotions = Acme.Promotions.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Employees;

public sealed partial class EmploymentContractApprovalEndpoint
{
    /// <summary>
    /// Approves an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.ApproveEmploymentContractResponse> ApproveEmploymentContract(ApiEmployees.ApproveEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveEmploymentContract for {Tenant}", requestContext.TenantId);
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
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesPositionValidationService.CheckPositionEligibilityAsync);
        var batch1 = (request.EmploymentContract?.Attachments ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.EmploymentContractAttachment>()).Select(async item =>
        {
            var input1 = new ApiRecruiting.SignalJobOpeningWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                JobOpening = new ApiRecruiting.JobOpening
                {
                    Attachments = { new ApiRecruiting.JobOpeningAttachment
                    {
                        Uri = (item.Uri ?? ""),
                    } },
                },
            };
            using var rpc1 = _recruitingJobOpeningWorkflowService.SignalJobOpeningWorkflowAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiRecruiting.SignalJobOpeningWorkflowResponse();
        var response = new ApiEmployees.ApproveEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Number = (request.EmploymentContract?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "ApproveEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.CommentEmploymentContractApprovalResponse> CommentEmploymentContractApproval(ApiEmployees.CommentEmploymentContractApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentEmploymentContractApproval for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiCorporateevents.ScheduleRegistrationExpiryResponse();
        foreach (var item in (request.EmploymentContract?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.EmploymentContractLine>()))
        {
            var input0 = new ApiCorporateevents.ScheduleRegistrationExpiryRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                    },
                    Limits = { new ApiCorporateevents.RegistrationLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc0 = _corporateeventsRegistrationLifecycleService.ScheduleRegistrationExpiryAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Bank = new ApiEmployees.EmploymentContractBankSettlement
                {
                    Bic = (reply0.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (reply0.Registration?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAuthorization.CountAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.CountAccessPoliciesRequest.Types.Filter
            {
                Labels = { (reply0.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        var command2 = new DispatchCommand<ApiAuthorization.CountAccessPoliciesRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _authorizationAccessPolicyQueryService.CountAccessPoliciesAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiEmployees.CommentEmploymentContractApprovalRequest, ApiMaintenance.SimulateWorkOrderRequest>(value => new ApiMaintenance.SimulateWorkOrderRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMaintenance.SimulateWorkOrderRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            WorkOrder = new ApiMaintenance.WorkOrder
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (reply0.Registration?.Contact?.Phone ?? ""),
                },
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _maintenanceWorkOrderCalculationService.SimulateWorkOrderAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.CommentEmploymentContractApprovalResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = (reply1.EmploymentContract?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                IssuedAt = (request.EmploymentContract?.IssuedAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "CommentEmploymentContractApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.DelegateEmploymentContractApprovalResponse> DelegateEmploymentContractApproval(ApiEmployees.DelegateEmploymentContractApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateEmploymentContractApproval for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.GetEmploymentContractRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContractId = (request.EmploymentContract?.EmploymentContractId ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmploymentContractValidationService.GetEmploymentContractRulesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.DelegateEmploymentContractApprovalToListEmployeeWorkflows1(reply0);
        using var rpc1 = _employeesEmployeeWorkflowService.ListEmployeeWorkflowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.SignalEmployeeWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (request.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
            },
        };
        var reply2 = await SendDelegateEmploymentContractApproval2Async(input2, token);
        var input3 = new ApiEmployees.RetryOrgUnitWorkflowRequest
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
                            Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (request.EmploymentContract?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                            Email = (reply2.Employee?.RelationshipManager?.Email ?? ""),
                        },
                    } },
                } },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _employeesOrgUnitWorkflowService.RetryOrgUnitWorkflowAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.DelegateEmploymentContractApprovalResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = (request.EmploymentContract?.Counterparty ?? new ApiEmployees.EmployeeRef()),
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
            Outcome = new ApiEmployees.DelegateEmploymentContractApprovalResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "DelegateEmploymentContractApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.EscalateEmploymentContractResponse> EscalateEmploymentContract(ApiEmployees.EscalateEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.StartOrgUnitWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.EmploymentContract?.BillingAddress?.Recipient ?? ""),
                        },
                    } },
                } },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.StartOrgUnitWorkflowRequest>();
        using var rpc0 = _employeesOrgUnitWorkflowService.StartOrgUnitWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiEmployees.PreviewEmploymentContractResponse();
        foreach (var item in (request.EmploymentContract?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployees.EmploymentContractLine>()))
        {
            var input1 = new ApiEmployees.PreviewEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Lines = { new ApiEmployees.EmploymentContractLine
                    {
                        Tax = new ApiEmployees.EmploymentContractLineTax
                        {
                            Rate = (item.Tax?.Rate ?? 0d),
                        },
                        UnitPrice = (item.UnitPrice ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc1 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiDocuments.PreviewDocumentRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Document = new ApiDocuments.Document
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (reply1.EmploymentContract?.Audit?.Etag ?? ""),
            },
            BillingAddress = new ApiCommon.PostalAddress
            {
                CountryCode = (reply0.OrgUnit?.Location?.CountryCode ?? ""),
                Region = (reply0.OrgUnit?.Location?.Region ?? ""),
            },
            Counterparty = new ApiDocuments.DocumentFolderRef
            {
                DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
            },
        };
        using var rpc2 = _documentsDocumentValidationService.PreviewDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.EscalateEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.OrgUnit?.Audit?.UpdatedBy ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.OrgUnit?.Manager?.DisplayName ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
            Outcome = new ApiEmployees.EscalateEmploymentContractResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "EscalateEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.GetEmploymentContractApprovalStatusResponse> GetEmploymentContractApprovalStatus(ApiEmployees.GetEmploymentContractApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmploymentContractId, "employment_contract_id");
        _logger.LogInformation("Handling GetEmploymentContractApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract", request.EmploymentContractId,
            "GetEmploymentContractApprovalStatus", "accepted", token);
        var input0 = new ApiEmployees.LintEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintEmploymentContractsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
        };
        using var rpc0 = _employeesEmploymentContractValidationService.LintEmploymentContractsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.GetEmploymentContractApprovalStatusResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                EmploymentContractId = (request.EmploymentContractId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists employment contracts a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListPendingEmploymentContractsResponse> ListPendingEmploymentContracts(ApiEmployees.ListPendingEmploymentContractsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingEmploymentContracts for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract", "batch",
            "ListPendingEmploymentContracts", "accepted", token);
        var response = new ApiEmployees.ListPendingEmploymentContractsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "ListPendingEmploymentContracts", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.RecallEmploymentContractResponse> RecallEmploymentContract(ApiEmployees.RecallEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFeatureflags.CountFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.CountFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagQueryService.CountFeatureFlagsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiAuthorization.WatchAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.WatchAccessPoliciesRequest.Types.Filter
            {
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.EmploymentContract?.Counterparty?.Email ?? ""),
            },
        };
        using var stream1 = _authorizationAccessPolicyService.WatchAccessPolicies(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchAccessPolicies frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiLedger.CheckLedgerAccountEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.CheckLedgerAccountEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                Card = new ApiLedger.LedgerAccountCardFunding
                {
                    Token = (request.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (request.EmploymentContract?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _ledgerLedgerAccountValidationService.CheckLedgerAccountEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.CountEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.CountEmployeesRequest.Types.Filter
            {
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc3 = _sendRecallEmploymentContract3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.RecallEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Notes = (request.EmploymentContract?.Notes ?? ""),
            },
            Outcome = new ApiEmployees.RecallEmploymentContractResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "RecallEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.RejectEmploymentContractResponse> RejectEmploymentContract(ApiEmployees.RejectEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ListEmployeeWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeeWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        var named0 = _registry.Get<ApiEmployees.EmployeeWorkflowService.EmployeeWorkflowServiceClient>("acme.employees.v1.EmployeeWorkflowService");
        using var rpc0 = named0.ListEmployeeWorkflowsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SubmitLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SubmitLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Card = new ApiLicensing.LicenseAssignmentCardSettlement
                {
                    Token = (request.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (request.EmploymentContract?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentApprovalService.SubmitLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.RejectEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Number = (request.EmploymentContract?.Number ?? ""),
            },
            Outcome = new ApiEmployees.RejectEmploymentContractResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "RejectEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an employment contract.
    /// </summary>
    public override async Task<ApiEmployees.SubmitEmploymentContractResponse> SubmitEmploymentContract(ApiEmployees.SubmitEmploymentContractRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitEmploymentContract for {Tenant}", requestContext.TenantId);
        if (request.EmploymentContract is { } record)
        {
            var snapshot = EmploymentContractMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPromotions.SearchPromotionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPromotions.SearchPromotionsRequest.Types.Filter
            {
                CreatedAfter = (request.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.EmploymentContract?.EmploymentContractId ?? ""),
                Region = (request.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiPromotions.SearchPromotionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _promotionsPromotionService.SearchPromotionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiEmployees.SubmitEmploymentContractRequest, ApiEmployees.GetEmployeeRequest>(value => new ApiEmployees.GetEmployeeRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            EmployeeId = (value.EmploymentContract?.Counterparty?.EmployeeId ?? ""),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _employeesEmployeeService.GetEmployeeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.RecallEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply1.Employee?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _employeesEmployeeApprovalService.RecallEmployeeAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.RecallEmployeeRequest>("acme.employees.v1.EmploymentContractApprovalService/SubmitEmploymentContract/2", input2.Clone()), token);
        var response = new ApiEmployees.SubmitEmploymentContractResponse
        {
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Employee?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.Employee?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    EmployeeId = (reply1.Employee?.EmployeeId ?? ""),
                },
                EmploymentContractId = (request.EmploymentContractId ?? ""),
                Notes = (request.EmploymentContract?.Notes ?? ""),
            },
            Outcome = new ApiEmployees.SubmitEmploymentContractResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employment_contract",
            "audit", "SubmitEmploymentContract", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.SignalEmployeeWorkflowResponse> SendDelegateEmploymentContractApproval2Async(ApiEmployees.SignalEmployeeWorkflowRequest request, CancellationToken token)
    {
        return await _employmentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port.SendAsync(request, token);
    }
}
