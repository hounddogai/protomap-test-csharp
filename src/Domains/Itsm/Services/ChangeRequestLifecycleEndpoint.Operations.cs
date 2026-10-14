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
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ChangeRequestLifecycleEndpoint
{
    /// <summary>
    /// Activates a change request.
    /// </summary>
    public override async Task<ApiItsm.ActivateChangeRequestResponse> ActivateChangeRequest(ApiItsm.ActivateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling ActivateChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "ActivateChangeRequest", "accepted", token);
        var input0 = new ApiItsm.EstimateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiItsm.EstimateChangeRequestRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _itsmChangeRequestCalculationService.EstimateChangeRequestAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiSearch.ExplainSearchIndexDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSearch.ExplainSearchIndexDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply0.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply0.ChangeRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
            SearchIndex = new ApiSearch.SearchIndex
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                    {
                        ApiItsm.ChangeRequest.OriginOneofCase.Customer => (reply0.ChangeRequest?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _searchSearchIndexValidationService.ExplainSearchIndexDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ActivateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
            Outcome = new ApiItsm.ActivateChangeRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ActivateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a change request.
    /// </summary>
    public override async Task<ApiItsm.ExpireChangeRequestResponse> ExpireChangeRequest(ApiItsm.ExpireChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling ExpireChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "ExpireChangeRequest", "accepted", token);
        var input0 = new ApiLicensing.ListLicenseAssignmentReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListLicenseAssignmentReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingLicenseAssignmentReportingService.ListLicenseAssignmentReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ExpireChangeRequestToPreviewSearchIndex1();
        using var rpc1 = _searchSearchIndexValidationService.PreviewSearchIndexAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ExpireChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.SearchIndex?.Owner?.Email ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
            Outcome = new ApiItsm.ExpireChangeRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ExpireChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.GetChangeRequestLifecycleResponse> GetChangeRequestLifecycle(ApiItsm.GetChangeRequestLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling GetChangeRequestLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "GetChangeRequestLifecycle", "accepted", token);
        var input0 = new ApiNotifications.ValidateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiNotifications.ValidateNotificationRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendGetChangeRequestLifecycle0Async(input0, token);
        var input1 = new ApiDatasets.BatchGetDatasetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _datasetsDatasetService.BatchGetDatasetsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.GetChangeRequestLifecycleResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Priority = (reply0.Notification?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a change request.
    /// </summary>
    public override async Task<ApiItsm.ReinstateChangeRequestResponse> ReinstateChangeRequest(ApiItsm.ReinstateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling ReinstateChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "ReinstateChangeRequest", "accepted", token);
        var input0 = new ApiItsm.CompareIncidentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiItsm.CompareIncidentOptionsRequest>();
        using var rpc0 = _itsmIncidentCalculationService.CompareIncidentOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CountChangeRequestTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.CountChangeRequestTotalsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Incident?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.Incident ?? new ApiItsm.Incident()).OriginCase switch
                {
                    ApiItsm.Incident.OriginOneofCase.Customer => (reply0.Incident?.Customer?.EmployeeId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _itsmChangeRequestReportingService.CountChangeRequestTotalsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ReinstateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Incident?.Assignee?.Email ?? ""),
                },
            },
            Outcome = new ApiItsm.ReinstateChangeRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ReinstateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a change request.
    /// </summary>
    public override async Task<ApiItsm.ScheduleChangeRequestExpiryResponse> ScheduleChangeRequestExpiry(ApiItsm.ScheduleChangeRequestExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleChangeRequestExpiry for {Tenant}", requestContext.TenantId);
        if (request.ChangeRequest is { } record)
        {
            var snapshot = ChangeRequestMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.FindOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                {
                    ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.IncidentId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _employeesOrgUnitQueryService.FindOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.BrowseEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.ChangeRequest ?? new ApiItsm.ChangeRequest()).OriginCase switch
                {
                    ApiItsm.ChangeRequest.OriginOneofCase.Customer => (request.ChangeRequest?.Customer?.IncidentId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeQueryService.SummarizeEmployeesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiEmployees.ResolveEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeQueryService.ResolveEmployeesAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.ScheduleChangeRequestExpiryResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Status = (request.ChangeRequest?.Status ?? (ApiItsm.ChangeRequestStatus)0),
            },
            Outcome = new ApiItsm.ScheduleChangeRequestExpiryResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ScheduleChangeRequestExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a change request.
    /// </summary>
    public override async Task<ApiItsm.SuspendChangeRequestResponse> SuspendChangeRequest(ApiItsm.SuspendChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling SuspendChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "SuspendChangeRequest", "accepted", token);
        var input0 = new ApiItsm.CountChangeRequestTotalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.CountChangeRequestTotalsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ChangeRequestId ?? ""),
            },
        };
        using var rpc0 = _itsmChangeRequestReportingService.CountChangeRequestTotalsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiIdentity.CloneUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
                UserId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc1 = _identityUserService.CloneUserAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiItsm.SuspendChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.User?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.User?.Audit?.Etag ?? ""),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
            Outcome = new ApiItsm.SuspendChangeRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "SuspendChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a change request.
    /// </summary>
    public override async Task<ApiItsm.TerminateChangeRequestResponse> TerminateChangeRequest(ApiItsm.TerminateChangeRequestRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling TerminateChangeRequest for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "TerminateChangeRequest", "accepted", token);
        var input0 = new ApiEmployees.BrowseOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient>("acme.employees.v1.OrgUnitQueryService");
        using var rpc0 = named0.BrowseOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RecordDocumentFolderVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordDocumentFolderVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Manager = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    },
                } },
                Manager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderHistoryService.RecordDocumentFolderVersionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ScorePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ScorePositionRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.DocumentFolder?.Location?.CountryCode ?? ""),
                    Locality = (reply1.DocumentFolder?.Location?.Locality ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.DocumentFolder?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply1.DocumentFolder?.Manager?.UserId ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiEmployees.ScorePositionRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _employeesPositionValidationService.ScorePositionAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.TerminateChangeRequestResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.Position?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
            Outcome = new ApiItsm.TerminateChangeRequestResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "TerminateChangeRequest", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiNotifications.ValidateNotificationResponse> SendGetChangeRequestLifecycle0Async(ApiNotifications.ValidateNotificationRequest request, CancellationToken token)
    {
        return await _changeRequestLifecycleEndpointSendGetChangeRequestLifecycle0Port.SendAsync(request, token);
    }
}
