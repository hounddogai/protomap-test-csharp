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
using ApiAssets = Acme.Assets.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;

namespace Acme.Workforce.Employees;

public sealed partial class OrgUnitWorkflowEndpoint
{
    /// <summary>
    /// Advances an org unit.
    /// </summary>
    public override async Task<ApiEmployees.AdvanceOrgUnitWorkflowResponse> AdvanceOrgUnitWorkflow(ApiEmployees.AdvanceOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.NormalizeAccessPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.NormalizeAccessPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _authorizationAccessPolicyValidationService.NormalizeAccessPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.ListFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ListFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply0.AccessPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.OrgUnit?.Location?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagService.ListFeatureFlagsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployees.AdvanceOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply0.AccessPolicy?.Audit?.UpdatedBy ?? ""),
                    },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    UserId = (reply0.AccessPolicy?.Owner?.UserId ?? ""),
                },
                OrgUnitId = (request.OrgUnitId ?? ""),
                ParentId = (request.OrgUnit?.ParentId ?? ""),
            },
            Outcome = new ApiEmployees.AdvanceOrgUnitWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "AdvanceOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an org unit.
    /// </summary>
    public override async Task<ApiEmployees.CancelOrgUnitWorkflowResponse> CancelOrgUnitWorkflow(ApiEmployees.CancelOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ValidateEmploymentContractRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.OrgUnit?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OrgUnit?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployees.CancelOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                        },
                    } },
                } },
                Location = (request.OrgUnit?.Location ?? new ApiCommon.PostalAddress()),
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "CancelOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an org unit.
    /// </summary>
    public override async Task<ApiEmployees.InspectOrgUnitWorkflowResponse> InspectOrgUnitWorkflow(ApiEmployees.InspectOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OrgUnitId, "org_unit_id");
        _logger.LogInformation("Handling InspectOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", request.OrgUnitId,
            "InspectOrgUnitWorkflow", "accepted", token);
        var input0 = new ApiAssets.QuoteFixedAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FixedAsset = new ApiAssets.FixedAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _assetsFixedAssetCalculationService.QuoteFixedAssetAsync);
        var response = new ApiEmployees.InspectOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply0.FixedAsset?.Audit?.CreatedBy ?? ""),
                    },
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (reply0.FixedAsset?.Address?.Region ?? ""),
                        },
                    } },
                } },
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists org units a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListOrgUnitWorkflowsResponse> ListOrgUnitWorkflows(ApiEmployees.ListOrgUnitWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListOrgUnitWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "ListOrgUnitWorkflows", "accepted", token);
        var input0 = new ApiHelpdesk.GenerateEscalationReportRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.GenerateEscalationReportRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _sendListOrgUnitWorkflows0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ListOrgUnitWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "ListOrgUnitWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an org unit.
    /// </summary>
    public override async Task<ApiEmployees.PauseOrgUnitWorkflowResponse> PauseOrgUnitWorkflow(ApiEmployees.PauseOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.SubmitCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.OrgUnit?.Location?.CountryCode ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient>("acme.corporateevents.v1.CorporateEventApprovalService");
        using var rpc0 = named0.SubmitCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.PauseOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.CorporateEvent?.Audit?.Etag ?? ""),
                },
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                        },
                    } },
                } },
                Depth = (request.OrgUnit?.Depth ?? 0),
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "PauseOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an org unit.
    /// </summary>
    public override async Task<ApiEmployees.ResumeOrgUnitWorkflowResponse> ResumeOrgUnitWorkflow(ApiEmployees.ResumeOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTimekeeping.RecallClockEventRequest
        {
            ClockEvent = new ApiTimekeeping.ClockEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTimekeeping.TimesheetRef
                {
                    DisplayName = (request.OrgUnit?.Manager?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTimekeeping.RecallClockEventRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _timekeepingClockEventApprovalService.RecallClockEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.RecallEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.OrgUnit?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.OrgUnit?.Location?.Locality ?? ""),
                    PostalCode = (request.OrgUnit?.Location?.PostalCode ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.OrgUnit?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiEmployees.RecallEmployeeRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _employeesEmployeeApprovalService.RecallEmployeeAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.PauseEmployeeWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Preferences = new ApiEmployees.EmployeePreferences
                {
                    Channels = { (reply1.Employee?.Preferences?.Channels ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        using var rpc2 = _employeesEmployeeWorkflowService.PauseEmployeeWorkflowAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.ListOfferReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ListOfferReportsRequest.Types.Filter
            {
                CreatedAfter = (reply1.Employee?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.OrgUnit?.Manager?.UserId ?? ""),
                Region = (request.OrgUnit?.Location?.Region ?? ""),
            },
        };
        using var rpc3 = _recruitingOfferReportingService.ListOfferReportsAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiEmployees.ResumeOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Attributes = { (reply1.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Attributes = { (reply0.ClockEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    } },
                } },
                Name = (request.OrgUnit?.Name ?? ""),
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "ResumeOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an org unit.
    /// </summary>
    public override async Task<ApiEmployees.RetryOrgUnitWorkflowResponse> RetryOrgUnitWorkflow(ApiEmployees.RetryOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.RetryOrgUnitWorkflowToBrowseOrgUnits0();
        using var rpc0 = _employeesOrgUnitQueryService.BrowseOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ResolveDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await SendRetryOrgUnitWorkflow1Async(input1, token);
        var input2 = new ApiEmployees.SubmitPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.OrgUnit?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.OrgUnit?.Audit?.CreatedBy ?? ""),
                    Etag = (request.OrgUnit?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var labels2 = input2.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input2.Context.Baggage.Clear();
        input2.Context.Baggage.Add(labels2);
        using var rpc2 = _employeesPositionApprovalService.SubmitPositionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OrgUnit?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.OrgUnit?.Location?.Recipient ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Tax = new ApiDocuments.DocumentLineTax
                    {
                        Amount = (reply2.Position ?? new ApiEmployees.Position()).ActionCase switch
                        {
                            ApiEmployees.Position.ActionOneofCase.Discount => (reply2.Position?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc3 = _documentsDocumentValidationService.ValidateDocumentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.RetryOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply2.Position?.Audit?.CreatedBy ?? ""),
                            UpdatedBy = (reply2.Position?.Audit?.UpdatedBy ?? ""),
                        },
                    } },
                } },
                OrgUnitId = (request.OrgUnitId ?? ""),
                ParentId = (request.OrgUnit?.ParentId ?? ""),
            },
            Outcome = new ApiEmployees.RetryOrgUnitWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "RetryOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an org unit.
    /// </summary>
    public override async Task<ApiEmployees.SignalOrgUnitWorkflowResponse> SignalOrgUnitWorkflow(ApiEmployees.SignalOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.RecalculateEmploymentContractsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.OrgUnit?.Location?.Locality ?? ""),
                    Recipient = (request.OrgUnit?.Location?.Recipient ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.OrgUnit?.Location?.PostalCode ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiEmployees.RecalculateEmploymentContractsRequest>(json0);
        using var rpc0 = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateExportRequest.Types.Filter
            {
                CreatedAfter = (request.OrgUnit?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.EmploymentContract?.Counterparty?.EmployeeId ?? ""),
                Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _documentsDocumentTemplateExportService.StreamDocumentTemplateExport(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateExport frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiEmployees.ExplainEmploymentContractDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.OrgUnit?.Location?.Region ?? ""),
                },
                Card = new ApiEmployees.EmploymentContractCardSettlement
                {
                    HolderName = (reply0.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (reply0.EmploymentContract?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Last4 = (reply0.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                    {
                        ApiEmployees.EmploymentContract.SettlementOneofCase.Card => (reply0.EmploymentContract?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _employeesEmploymentContractValidationService.ExplainEmploymentContractDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiFeatureflags.BatchGetFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagService.BatchGetFeatureFlagsAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployees.SignalOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply2.EmploymentContract?.BillingAddress?.Locality ?? ""),
                },
                OrgUnitId = (request.OrgUnitId ?? ""),
                Status = (request.OrgUnit?.Status ?? (ApiEmployees.OrgUnitStatus)0),
            },
            Outcome = new ApiEmployees.SignalOrgUnitWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply3.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "SignalOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an org unit.
    /// </summary>
    public override async Task<ApiEmployees.StartOrgUnitWorkflowResponse> StartOrgUnitWorkflow(ApiEmployees.StartOrgUnitWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartOrgUnitWorkflow for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PeekOrgUnitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            OrgUnitId = (request.OrgUnit?.OrgUnitId ?? ""),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesOrgUnitQueryService.PeekOrgUnitAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.SubmitEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SubmitEmployeeRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.OrgUnit?.Manager?.DisplayName ?? ""),
                },
            },
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (reply0.OrgUnit?.Manager?.UserId ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _employeesEmployeeApprovalService.SubmitEmployeeAsync);
        var response = new ApiEmployees.StartOrgUnitWorkflowResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Attributes = { (request.OrgUnit?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = (reply1.Employee?.Audit?.UpdatedBy ?? ""),
                    },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Employee?.PrimaryAddress?.PostalCode ?? ""),
                },
                OrgUnitId = (request.OrgUnitId ?? ""),
                ParentId = (reply0.OrgUnit?.ParentId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "StartOrgUnitWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.ResolveDocumentFoldersResponse> SendRetryOrgUnitWorkflow1Async(ApiDocuments.ResolveDocumentFoldersRequest request, CancellationToken token)
    {
        return await _orgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port.SendAsync(request, token);
    }
}
