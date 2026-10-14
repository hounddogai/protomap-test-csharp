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
using ApiBudgeting = Acme.Budgeting.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class IncidentApprovalEndpoint
{
    /// <summary>
    /// Approves an incident.
    /// </summary>
    public override async Task<ApiItsm.ApproveIncidentResponse> ApproveIncident(ApiItsm.ApproveIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.NormalizeNotificationRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Notification = new ApiNotifications.Notification
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedAt = (request.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                UpdatedBy = (request.Incident?.Audit?.UpdatedBy ?? ""),
            },
        };
        using var rpc0 = _notificationsNotificationValidationService.NormalizeNotificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.CountMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.CountMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Sla?.DueAt ?? new Timestamp()),
                OwnerId = (request.Incident ?? new ApiItsm.Incident()).OriginCase switch
                {
                    ApiItsm.Incident.OriginOneofCase.Customer => (request.Incident?.Customer?.EmployeeId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _metricsMetricSeriesQueryService.CountMetricSeriesesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ApproveIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Requester = (request.Incident?.Requester ?? new ApiCommon.ContactInfo()),
                Sla = new ApiItsm.IncidentSla
                {
                    PausedFor = (reply0.Notification?.Sla?.PausedFor ?? new Duration()),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ApproveIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Comments an incident.
    /// </summary>
    public override async Task<ApiItsm.CommentIncidentApprovalResponse> CommentIncidentApproval(ApiItsm.CommentIncidentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CommentIncidentApproval for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExportEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Incident?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Incident?.Assignee?.UserId ?? ""),
            },
        };
        using var stream0 = _employeesEmployeeQueryService.ExportEmployees(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportEmployees frame {Type}", change0.GetType().Name);
        }
        var reply1 = new ApiItsm.CountServiceAssetTotalsResponse();
        foreach (var item in (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiItsm.CountServiceAssetTotalsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiItsm.CountServiceAssetTotalsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                },
            };
            using var rpc1 = _itsmServiceAssetReportingService.CountServiceAssetTotalsAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var batch2 = (request.Incident?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.IncidentComment>()).Select(async item =>
        {
            var input2 = new ApiEmployees.BrowseEmployeesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc2 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            return reply2;
        }).ToArray();
        var replies2 = await Task.WhenAll(batch2);
        var reply2 = replies2.LastOrDefault() ?? new ApiEmployees.BrowseEmployeesResponse();
        var input3 = new ApiItsm.StreamServiceAssetMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Incident?.Assignee?.Email ?? ""),
                Name = (request.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Incident?.Requester?.Phone ?? ""),
            },
        };
        using var stream3 = _itsmServiceAssetReportingService.StreamServiceAssetMetrics(input3, cancellationToken: token);
        await foreach (var change3 in stream3.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamServiceAssetMetrics frame {Type}", change3.GetType().Name);
        }
        var response = new ApiItsm.CommentIncidentApprovalResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Requester = (request.Incident?.Requester ?? new ApiCommon.ContactInfo()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "CommentIncidentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates an incident.
    /// </summary>
    public override async Task<ApiItsm.DelegateIncidentApprovalResponse> DelegateIncidentApproval(ApiItsm.DelegateIncidentApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateIncidentApproval for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named0 = _registry.Get<ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient>("acme.employees.v1.OrgUnitQueryService");
        using var rpc0 = named0.ResolveOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor1 = new Method<ApiEmployees.ResolveEmployeesRequest, ApiEmployees.ResolveEmployeesResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmployeeQueryService", "ResolveEmployees",
            Wire.Marshaller<ApiEmployees.ResolveEmployeesRequest>(), Wire.Marshaller<ApiEmployees.ResolveEmployeesResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.DelegateIncidentApprovalResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Subject = (request.Incident?.Subject ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "DelegateIncidentApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates an incident.
    /// </summary>
    public override async Task<ApiItsm.EscalateIncidentResponse> EscalateIncident(ApiItsm.EscalateIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SyncProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SyncProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc0 = _itsmProblemSyncService.SyncProblemsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.EscalateIncidentRequest, ApiItsm.ScheduleServiceAssetExpiryRequest>(value => new ApiItsm.ScheduleServiceAssetExpiryRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (value.Details?.Approver?.UserId ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmServiceAssetLifecycleService.ScheduleServiceAssetExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.EscalateIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ServiceAsset?.Audit?.Etag ?? ""),
                },
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.Problem?.Requester?.PreferredChannel ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "EscalateIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an incident.
    /// </summary>
    public override async Task<ApiItsm.GetIncidentApprovalStatusResponse> GetIncidentApprovalStatus(ApiItsm.GetIncidentApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.IncidentId, "incident_id");
        _logger.LogInformation("Handling GetIncidentApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", request.IncidentId,
            "GetIncidentApprovalStatus", "accepted", token);
        var input0 = new ApiEmployees.BrowseEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.IncidentId ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.GetIncidentApprovalStatusResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists incidents a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListPendingIncidentsResponse> ListPendingIncidents(ApiItsm.ListPendingIncidentsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingIncidents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "incident", "batch",
            "ListPendingIncidents", "accepted", token);
        var input0 = request.ListPendingIncidentsToGenerateProblemReport0();
        using var rpc0 = _itsmProblemReportingService.GenerateProblemReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ListPendingIncidentsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "ListPendingIncidents", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls an incident.
    /// </summary>
    public override async Task<ApiItsm.RecallIncidentResponse> RecallIncident(ApiItsm.RecallIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.RecalculateCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Incident?.Audit?.Etag ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Incident?.Requester?.Email ?? ""),
                        Phone = (request.Incident?.Requester?.Phone ?? ""),
                        PreferredChannel = (request.Incident?.Requester?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var reply0 = await SendRecallIncident0Async(input0, token);
        var input1 = new ApiItsm.SearchIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.SearchIncidentsRequest.Types.Filter
            {
                CreatedAfter = (reply0.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _itsmIncidentService.SearchIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiBudgeting.CalculateBudgetRequest
        {
            Budget = new ApiBudgeting.Budget
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Incident?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBudgeting.CalculateBudgetRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.CorporateEvent?.Location?.CountryCode ?? ""),
                },
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiBudgeting.CalculateBudgetRequest>();
        using var rpc2 = _budgetingBudgetCalculationService.CalculateBudgetAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.RecallIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.CorporateEvent?.Organizer?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
                IncidentId = (request.IncidentId ?? ""),
                Requester = (request.Incident?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Outcome = new ApiItsm.RecallIncidentResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "RecallIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects an incident.
    /// </summary>
    public override async Task<ApiItsm.RejectIncidentResponse> RejectIncident(ApiItsm.RejectIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentFolderUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Incident?.Requester?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentFolderNotificationService.SubscribeDocumentFolderUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ResolveEmployeesRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc1 = _employeesEmployeeQueryService.ResolveEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.RejectIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
            },
            Outcome = new ApiItsm.RejectIncidentResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "RejectIncident", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits an incident.
    /// </summary>
    public override async Task<ApiItsm.SubmitIncidentResponse> SubmitIncident(ApiItsm.SubmitIncidentRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitIncident for {Tenant}", requestContext.TenantId);
        if (request.Incident is { } record)
        {
            var snapshot = IncidentMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Incident?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _employeesPositionValidationService.PreviewPositionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.RecalculateServiceAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Incident?.Audit?.CreatedBy ?? ""),
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Position?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmServiceAssetCalculationService.RecalculateServiceAssetsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.SubmitIncidentResponse
        {
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.ServiceAsset?.Owner?.Email ?? ""),
                },
                IncidentId = (request.IncidentId ?? ""),
                Priority = (request.Incident?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
            Outcome = new ApiItsm.SubmitIncidentResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "incident",
            "audit", "SubmitIncident", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.RecalculateCorporateEventsResponse> SendRecallIncident0Async(ApiCorporateevents.RecalculateCorporateEventsRequest request, CancellationToken token)
    {
        return await _incidentApprovalEndpointSendRecallIncident0Port.SendAsync(request, token);
    }
}
