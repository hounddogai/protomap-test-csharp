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
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class IncidentWorkflowEndpoint
{
    /// <summary>
    /// Advances an incident.
    /// </summary>
    public override async Task<ApiItsm.AdvanceIncidentWorkflowResponse> AdvanceIncidentWorkflow(ApiItsm.AdvanceIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CommentChangeRequestApprovalRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Channel = new ApiItsm.ChangeRequestChannelOrigin
                {
                    Address = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Channel => (request.Incident?.Channel?.Address ?? ""),
                        _ => ""
                    },
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _itsmChangeRequestApprovalService.CommentChangeRequestApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ActivateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ActivateIncidentRequest.Types.Details
            {
                NewOwner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ChangeRequest?.Assignee?.UserId ?? ""),
                },
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                    Phone = (reply0.ChangeRequest?.Requester?.Phone ?? ""),
                },
            },
            IncidentId = (request.IncidentId ?? ""),
        };
        var descriptor1 = new Method<ApiItsm.ActivateIncidentRequest, ApiItsm.ActivateIncidentResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".IncidentLifecycleService", "ActivateIncident",
            Wire.Marshaller<ApiItsm.ActivateIncidentRequest>(), Wire.Marshaller<ApiItsm.ActivateIncidentResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.AdvanceIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                CustomFields = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                IncidentId = (reply0.ChangeRequest?.Customer?.IncidentId ?? ""),
                Requester = (request.Incident?.Requester ?? new ApiCommon.ContactInfo()),
                Subject = (reply1.Incident?.Subject ?? ""),
            },
            Outcome = new ApiItsm.AdvanceIncidentWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "AdvanceIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels an incident.
    /// </summary>
    public override async Task<ApiItsm.CancelIncidentWorkflowResponse> CancelIncidentWorkflow(ApiItsm.CancelIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.FindEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                {
                    ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.EmployeeId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _employeesEmployeeQueryService.FindEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.CancelIncidentWorkflowRequest, ApiItsm.PreviewChangeRequestRequest>(value => new ApiItsm.PreviewChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (value.Incident?.Assignee?.Email ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmChangeRequestValidationService.PreviewChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CancelIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
                Sla = new ApiItsm.IncidentSla
                {
                    PausedFor = (reply1.ChangeRequest?.Sla?.PausedFor ?? new Duration()),
                },
            },
            Outcome = new ApiItsm.CancelIncidentWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CancelIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.InspectIncidentWorkflowResponse> InspectIncidentWorkflow(ApiItsm.InspectIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling InspectIncidentWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "InspectIncidentWorkflow", "accepted", token);
        var input0 = new ApiItsm.QuoteProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmProblemCalculationService.QuoteProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.InspectIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Sla = new ApiItsm.IncidentSla
                {
                    DueAt = (reply0.Problem?.Sla?.DueAt ?? new Timestamp()),
                    PolicyId = (reply0.Problem?.Sla?.PolicyId ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListIncidentWorkflowsResponse> ListIncidentWorkflows(ApiItsm.ListIncidentWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListIncidentWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "ListIncidentWorkflows", "accepted", token);
        var input0 = request.ListIncidentWorkflowsToSummarizeOrgUnits0();
        using var rpc0 = _employeesOrgUnitQueryService.SummarizeOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ListIncidentWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ListIncidentWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses an incident.
    /// </summary>
    public override async Task<ApiItsm.PauseIncidentWorkflowResponse> PauseIncidentWorkflow(ApiItsm.PauseIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RetryDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RetryDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await SendPauseIncidentWorkflow0Async(input0, token);
        var input1 = new ApiSearch.CountSearchIndexesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.CountSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.DevicePolicy?.Owner?.UserId ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _searchSearchIndexQueryService.CountSearchIndexesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListDocumentFolderVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderVersionsRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.Incident?.Sla?.PolicyId ?? ""),
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiDocuments.ListDocumentFolderVersionsRequest>();
        using var rpc2 = _documentsDocumentFolderHistoryService.ListDocumentFolderVersionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiDocuments.ListDocumentExportsResponse();
        foreach (var item in (request.Incident?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiDocuments.ListDocumentExportsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentExportsRequest.Types.Filter
                {
                    CreatedAfter = (item.ChangedAt ?? new Timestamp()),
                    Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (request.Incident?.Customer?.EmployeeId ?? ""),
                },
            };
            using var rpc3 = _documentsDocumentExportService.ListDocumentExportsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiItsm.PauseIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
            },
            Outcome = new ApiItsm.PauseIncidentWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "PauseIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes an incident.
    /// </summary>
    public override async Task<ApiItsm.ResumeIncidentWorkflowResponse> ResumeIncidentWorkflow(ApiItsm.ResumeIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NormalizeDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Incident?.Requester?.PreferredChannel ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _documentsDocumentValidationService.NormalizeDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiItsm.SyncProblemsResponse();
        foreach (var item in (request.Incident?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentStatusChange>()))
        {
            var input1 = new ApiItsm.SyncProblemsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiItsm.SyncProblemsRequest.Types.Details
                {
                    Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                Problem = new ApiItsm.Problem
                {
                    History = { new ApiItsm.ProblemStatusChange
                    {
                        ChangedBy = (item.ChangedBy ?? ""),
                        FromStatus = (item.FromStatus ?? ""),
                        ToStatus = (item.ToStatus ?? ""),
                    } },
                },
            };
            using var rpc1 = _itsmProblemSyncService.SyncProblemsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiItsm.ResumeIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = (request.Incident?.Assignee ?? new ApiIdentity.UserRef()),
                Comments = { new ApiItsm.IncidentComment
                {
                    CreatedAt = (reply0.Document?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                IncidentId = (request.IncidentId ?? ""),
            },
            Outcome = new ApiItsm.ResumeIncidentWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ResumeIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys an incident.
    /// </summary>
    public override async Task<ApiItsm.RetryIncidentWorkflowResponse> RetryIncidentWorkflow(ApiItsm.RetryIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Incident?.Audit?.CreatedBy ?? ""),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.Incident?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingLicenseAssignmentApprovalService.DelegateLicenseAssignmentApprovalAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiItsm.RecalculateProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.RecalculateProblemsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.LicenseAssignment?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Problem = new ApiItsm.Problem
            {
                Channel = new ApiItsm.ProblemChannelOrigin
                {
                    Channel = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                    {
                        ApiItsm.Incident.OriginOneofCase.Channel => (request.Incident?.Channel?.Channel ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _itsmProblemCalculationService.RecalculateProblemsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.RetryIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = (request.Incident?.Assignee ?? new ApiIdentity.UserRef()),
                IncidentId = (request.IncidentId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Problem?.Assignee?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "RetryIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals an incident.
    /// </summary>
    public override async Task<ApiItsm.SignalIncidentWorkflowResponse> SignalIncidentWorkflow(ApiItsm.SignalIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.FindOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Incident?.Assignee?.UserId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesOrgUnitQueryService.FindOrgUnitsAsync);
        var response = new ApiItsm.SignalIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Requester = (request.Incident?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Outcome = new ApiItsm.SignalIncidentWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "SignalIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts an incident.
    /// </summary>
    public override async Task<ApiItsm.StartIncidentWorkflowResponse> StartIncidentWorkflow(ApiItsm.StartIncidentWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartIncidentWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLedger.PreviewLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Incident?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _sendStartIncidentWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(async item =>
        {
            var input1 = new ApiLicensing.ListLicenseAssignmentWorkflowsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiLicensing.ListLicenseAssignmentWorkflowsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                },
            };
            using var rpc1 = _licensingLicenseAssignmentWorkflowService.ListLicenseAssignmentWorkflowsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiLicensing.ListLicenseAssignmentWorkflowsResponse();
        var response = new ApiItsm.StartIncidentWorkflowResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "StartIncidentWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.RetryDevicePolicyWorkflowResponse> SendPauseIncidentWorkflow0Async(ApiDevices.RetryDevicePolicyWorkflowRequest request, CancellationToken token)
    {
        return await _incidentWorkflowEndpointSendPauseIncidentWorkflow0Port.SendAsync(request, token);
    }
}
