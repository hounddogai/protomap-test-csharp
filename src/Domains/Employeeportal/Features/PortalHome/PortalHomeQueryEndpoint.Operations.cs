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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class PortalHomeQueryEndpoint
{
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.BrowsePortalHomesResponse> BrowsePortalHomes(ApiEmployeeportal.BrowsePortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowsePortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "BrowsePortalHomes", "accepted", token);
        var response = new ApiEmployeeportal.BrowsePortalHomesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "BrowsePortalHomes", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching portal home.
    /// </summary>
    public override async Task ExportPortalHomes(ApiEmployeeportal.ExportPortalHomesRequest request, IServerStreamWriter<ApiEmployeeportal.ExportPortalHomesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "ExportPortalHomes", "accepted", token);
        var input0 = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.DisconnectSelfServiceRequestSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (request.Filter?.OwnerId ?? ""),
                ReceivedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc0 = _employeeportalSelfServiceRequestIntegrationService.DisconnectSelfServiceRequestSourceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListSelfServiceRequestSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeeportalSelfServiceRequestNotificationService.ListSelfServiceRequestSubscriptionsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Outcome?.Message ?? ""),
            },
            PortalHomeId = (reply0.SelfServiceRequest ?? new ApiEmployeeportal.SelfServiceRequest()).OriginCase switch
            {
                ApiEmployeeportal.SelfServiceRequest.OriginOneofCase.Customer => (reply0.SelfServiceRequest?.Customer?.PortalHomeId ?? ""),
                _ => ""
            },
        };
        using var rpc2 = _employeeportalPortalHomeIntegrationService.AcknowledgePortalHomeEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployeeportal.RetryAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.PortalHome?.Audit?.Etag ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.RetryAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.SelfServiceRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _employeeportalAnnouncementWorkflowService.RetryAnnouncementWorkflowAsync);
        var response = new ApiEmployeeportal.ExportPortalHomesResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.SelfServiceRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SelfServiceRequest?.Requester?.Email ?? ""),
                },
                Sla = new ApiEmployeeportal.PortalHomeSla
                {
                    DueAt = (reply3.Announcement?.DueAt ?? new Timestamp()),
                    PausedFor = (reply2.PortalHome?.Sla?.PausedFor ?? new Duration()),
                    PolicyId = (reply2.PortalHome?.Sla?.PolicyId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ExportPortalHomes", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.FindPortalHomesResponse> FindPortalHomes(ApiEmployeeportal.FindPortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "FindPortalHomes", "accepted", token);
        var input0 = new ApiEmployeeportal.ListPortalHomeSourcesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployeeportal.ListPortalHomeSourcesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var rpc0 = _sendFindPortalHomes0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.FindPortalHomesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "FindPortalHomes", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.LookupPortalHomeResponse> LookupPortalHome(ApiEmployeeportal.LookupPortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PortalHomeId, "portal_home_id");
        _logger.LogInformation("Handling LookupPortalHome for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "LookupPortalHome", "accepted", token);
        var response = new ApiEmployeeportal.LookupPortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.PeekPortalHomeResponse> PeekPortalHome(ApiEmployeeportal.PeekPortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PortalHomeId, "portal_home_id");
        _logger.LogInformation("Handling PeekPortalHome for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "PeekPortalHome", "accepted", token);
        var input0 = new ApiBenefits.StartBenefitPlanWorkflowRequest
        {
            BenefitPlan = new ApiBenefits.BenefitPlan
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Parameters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiBenefits.StartBenefitPlanWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient>("acme.benefits.v1.BenefitPlanWorkflowService");
        using var rpc0 = named0.StartBenefitPlanWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.StreamSignatureExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamSignatureExportRequest.Types.Filter
            {
                CreatedAfter = (reply0.BenefitPlan?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var stream1 = _documentsSignatureExportService.StreamSignatureExport(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamSignatureExport frame {Type}", change1.GetType().Name);
        }
        var response = new ApiEmployeeportal.PeekPortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.BenefitPlan?.Audit?.UpdatedBy ?? ""),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.SuggestPortalHomesResponse> SuggestPortalHomes(ApiEmployeeportal.SuggestPortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "SuggestPortalHomes", "accepted", token);
        var input0 = new ApiEmployees.RecallEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.RecallEmploymentContractRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.RecallEmploymentContractRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmploymentContractApprovalService.RecallEmploymentContractAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.SuggestPortalHomesResponse
        {
            PortalHomes = { new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.EmploymentContract?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "SuggestPortalHomes", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.SummarizePortalHomesResponse> SummarizePortalHomes(ApiEmployeeportal.SummarizePortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizePortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "SummarizePortalHomes", "accepted", token);
        var profile0 = new MappingProfile<ApiEmployeeportal.SummarizePortalHomesRequest, ApiEmployeeportal.StartAnnouncementWorkflowRequest>(value => new ApiEmployeeportal.StartAnnouncementWorkflowRequest
        {
            Annotations = { (value.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Announcement = new ApiEmployeeportal.Announcement
            {
                Counterparty = new ApiEmployeeportal.PortalHomeRef
                {
                    PortalHomeId = (value.PortalHomeId ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _employeeportalAnnouncementWorkflowService.StartAnnouncementWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgePortalHomeEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            PortalHomeId = (reply0.Announcement?.Counterparty?.PortalHomeId ?? ""),
        };
        using var rpc1 = _employeeportalPortalHomeIntegrationService.AcknowledgePortalHomeEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.SummarizePortalHomesToBrowseAuditEvents2(reply0, reply1);
        using var rpc2 = _auditAuditEventQueryService.BrowseAuditEventsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.SummarizePortalHomesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "SummarizePortalHomes", (request.Reason ?? ""), token);
        return response;
    }
}
