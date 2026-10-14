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
using ApiExperiments = Acme.Experiments.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLoyalty = Acme.Loyalty.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employees;

public sealed partial class OrgUnitApprovalEndpoint
{
    /// <summary>
    /// Approves an org unit.
    /// </summary>
    public override async Task<ApiEmployees.ApproveOrgUnitResponse> ApproveOrgUnit(ApiEmployees.ApproveOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveOrgUnit for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.CalculateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OrgUnit?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiFeatureflags.SuggestFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.SuggestFeatureFlagsRequest.Types.Filter
            {
                Region = (request.OrgUnit?.Location?.Region ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _featureflagsFeatureFlagQueryService.SuggestFeatureFlagsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployees.CreateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OrgUnit?.Audit?.CreatedBy ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OrgUnit?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _employeesEmployeeService.CreateEmployeeAsync);
        var response = new ApiEmployees.ApproveOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.EmploymentContract?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (reply2.Employee?.PrimaryAddress?.Region ?? ""),
                },
                OrgUnitId = (request.OrgUnitId ?? ""),
                ParentId = (request.OrgUnit?.ParentId ?? ""),
            },
            Outcome = new ApiEmployees.ApproveOrgUnitResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "ApproveOrgUnit", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an org unit.
    /// </summary>
    public override async Task<ApiEmployees.CommentOrgUnitApprovalResponse> CommentOrgUnitApproval(ApiEmployees.CommentOrgUnitApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentOrgUnitApproval for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PreviewEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.OrgUnit?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OrgUnit?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.OrgUnit?.Location?.Locality ?? ""),
                },
            },
        };
        using var rpc0 = _sendCommentOrgUnitApproval0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiEmployees.PreviewEmploymentContractRequest>("acme.employees.v1.OrgUnitApprovalService/CommentOrgUnitApproval/0", input0.Clone()), token);
        var response = new ApiEmployees.CommentOrgUnitApprovalResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Depth = (request.OrgUnit?.Depth ?? 0),
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "CommentOrgUnitApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an org unit.
    /// </summary>
    public override async Task<ApiEmployees.DelegateOrgUnitApprovalResponse> DelegateOrgUnitApproval(ApiEmployees.DelegateOrgUnitApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateOrgUnitApproval for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
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
                    CreatedAt = (request.OrgUnit?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.LintPositionsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.CompareEmploymentContractOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.OrgUnit?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.OrgUnit?.Location?.Recipient ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _employeesEmploymentContractCalculationService.CompareEmploymentContractOptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command2 = new DispatchCommand<ApiEmployees.ResolveEmployeesRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _employeesEmployeeQueryService.ResolveEmployeesAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiEmployees.DelegateOrgUnitApprovalRequest, ApiEmployees.SimulateEmploymentContractRequest>(value => new ApiEmployees.SimulateEmploymentContractRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (value.OrgUnit?.Location?.PostalCode ?? ""),
                },
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _employeesEmploymentContractCalculationService.SimulateEmploymentContractAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.DelegateOrgUnitApprovalResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply0.Position?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (reply3.EmploymentContract?.BillingAddress?.Region ?? ""),
                        },
                    } },
                    Location = new ApiCommon.PostalAddress
                    {
                        CountryCode = (reply1.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                    },
                } },
                Location = (request.OrgUnit?.Location ?? new ApiCommon.PostalAddress()),
                Manager = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                },
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
            Outcome = new ApiEmployees.DelegateOrgUnitApprovalResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "DelegateOrgUnitApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an org unit.
    /// </summary>
    public override async Task<ApiEmployees.EscalateOrgUnitResponse> EscalateOrgUnit(ApiEmployees.EscalateOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateOrgUnit for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.LintPositionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintPositionsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.OrgUnit?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.OrgUnit?.Manager?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesPositionValidationService.LintPositionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.EscalateOrgUnitToPreviewEmploymentContract1();
        using var rpc1 = _employeesEmploymentContractValidationService.PreviewEmploymentContractAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentFolderConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.OrgUnit?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.EmploymentContract?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Position?.Owner?.UserId ?? ""),
                Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
            },
        };
        var reply2 = await SendEscalateOrgUnit2Async(input2, token);
        var response = new ApiEmployees.EscalateOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Attributes = { (request.OrgUnit?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            PostalCode = (reply1.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply0.Position?.Owner?.Email ?? ""),
                        },
                    } },
                } },
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "EscalateOrgUnit", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an org unit.
    /// </summary>
    public override async Task<ApiEmployees.GetOrgUnitApprovalStatusResponse> GetOrgUnitApprovalStatus(ApiEmployees.GetOrgUnitApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OrgUnitId, "org_unit_id");
        _logger.LogInformation("Handling GetOrgUnitApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", request.OrgUnitId,
            "GetOrgUnitApprovalStatus", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentTemplateVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.OrgUnitId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiFeatureflags.LintFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiFeatureflags.LintFeatureFlagsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            FeatureFlag = new ApiFeatureflags.FeatureFlag
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Parameters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiFeatureflags.LintFeatureFlagsRequest>();
        using var rpc1 = _featureflagsFeatureFlagValidationService.LintFeatureFlagsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.GetOrgUnitApprovalStatusResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (reply1.FeatureFlag?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            Email = (reply1.FeatureFlag?.Owner?.Email ?? ""),
                        },
                    } },
                } },
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists org units a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListPendingOrgUnitsResponse> ListPendingOrgUnits(ApiEmployees.ListPendingOrgUnitsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingOrgUnits for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "org_unit", "batch",
            "ListPendingOrgUnits", "accepted", token);
        var input0 = new ApiLoyalty.FindLoyaltyMembersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLoyalty.FindLoyaltyMembersRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
            OrderBy = (request.OrderBy ?? ""),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLoyalty.FindLoyaltyMembersRequest>(json0);
        using var rpc0 = _loyaltyLoyaltyMemberQueryService.FindLoyaltyMembersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ListPendingOrgUnitsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "ListPendingOrgUnits", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an org unit.
    /// </summary>
    public override async Task<ApiEmployees.RecallOrgUnitResponse> RecallOrgUnit(ApiEmployees.RecallOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallOrgUnit for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CancelTicketWorkflowRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Ticket = new ApiHelpdesk.Ticket
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.OrgUnit?.Audit?.Etag ?? ""),
                UpdatedAt = (request.OrgUnit?.Audit?.UpdatedAt ?? new Timestamp()),
            },
            Customer = new ApiHelpdesk.KnowledgeArticleRef
            {
                DisplayName = (request.OrgUnit?.Manager?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _helpdeskTicketWorkflowService.CancelTicketWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiScheduling.SignalAvailabilityWindowWorkflowRequest
        {
            AvailabilityWindow = new ApiScheduling.AvailabilityWindow
            {
                Participants = { new ApiScheduling.AvailabilityWindowParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (reply0.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                        PreferredChannel = (reply0.Ticket?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _schedulingAvailabilityWindowWorkflowService.SignalAvailabilityWindowWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.RecallOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Children = { new ApiEmployees.OrgUnit
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            CountryCode = (reply1.AvailabilityWindow?.Location?.CountryCode ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (reply0.Ticket?.Customer?.DisplayName ?? ""),
                        },
                    } },
                } },
                OrgUnitId = (request.OrgUnitId ?? ""),
                Status = (request.OrgUnit?.Status ?? (ApiEmployees.OrgUnitStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "RecallOrgUnit", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an org unit.
    /// </summary>
    public override async Task<ApiEmployees.RejectOrgUnitResponse> RejectOrgUnit(ApiEmployees.RejectOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectOrgUnit for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ValidateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.OrgUnit?.Manager?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractValidationService.ValidateEmploymentContractAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.EscalateEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.OrgUnit?.Location?.CountryCode ?? ""),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeApprovalService.EscalateEmployeeAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiScheduling.PauseRosterWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Roster = new ApiScheduling.Roster
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Employee?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _schedulingRosterWorkflowService.PauseRosterWorkflowAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiExperiments.BrowseExperimentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiExperiments.BrowseExperimentsRequest.Types.Filter
            {
                CreatedAfter = (reply1.Employee?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (request.OrgUnit?.Location?.Region ?? ""),
            },
        };
        using var rpc3 = _experimentsExperimentQueryService.BrowseExperimentsAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiEmployees.RejectOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply1.Employee?.Audit?.CreatedBy ?? ""),
                        UpdatedBy = (reply2.Roster?.Audit?.UpdatedBy ?? ""),
                    },
                    Location = new ApiCommon.PostalAddress
                    {
                        Region = (reply2.Roster?.BillingAddress?.Region ?? ""),
                    },
                } },
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
                OrgUnitId = (request.OrgUnitId ?? ""),
                ParentId = (request.OrgUnit?.ParentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "RejectOrgUnit", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an org unit.
    /// </summary>
    public override async Task<ApiEmployees.SubmitOrgUnitResponse> SubmitOrgUnit(ApiEmployees.SubmitOrgUnitRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitOrgUnit for {Tenant}", requestContext.TenantId);
        if (request.OrgUnit is { } record)
        {
            var snapshot = OrgUnitMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiDocuments.DocumentQueryService.DocumentQueryServiceClient>("acme.documents.v1.DocumentQueryService");
        using var rpc0 = named0.ResolveDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ExportEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportEmployeesRequest.Types.Filter
            {
                Region = (request.OrgUnit?.Location?.Region ?? ""),
            },
        };
        using var stream1 = _employeesEmployeeQueryService.ExportEmployees(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportEmployees frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewPositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.OrgUnit?.Manager?.Email ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiEmployees.PreviewPositionRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _employeesPositionValidationService.PreviewPositionAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiEmployees.SubmitOrgUnitRequest, ApiEmployees.ResolveOrgUnitsRequest>(value => new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _employeesOrgUnitQueryService.ResolveOrgUnitsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployees.SubmitOrgUnitResponse
        {
            OrgUnit = new ApiEmployees.OrgUnit
            {
                Children = { new ApiEmployees.OrgUnit
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply2.Position?.Audit?.CreatedAt ?? new Timestamp()),
                    },
                } },
                Manager = (request.OrgUnit?.Manager ?? new ApiIdentity.UserRef()),
                OrgUnitId = (request.OrgUnitId ?? ""),
            },
            Outcome = new ApiEmployees.SubmitOrgUnitResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "org_unit",
            "audit", "SubmitOrgUnit", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDocuments.ListDocumentFolderConflictsResponse> SendEscalateOrgUnit2Async(ApiDocuments.ListDocumentFolderConflictsRequest request, CancellationToken token)
    {
        return await _orgUnitApprovalEndpointSendEscalateOrgUnit2Port.SendAsync(request, token);
    }
}
