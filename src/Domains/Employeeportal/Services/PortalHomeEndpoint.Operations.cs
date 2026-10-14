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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class PortalHomeEndpoint
{
    /// <summary>
    /// Returns several portal homes at once.
    /// </summary>
    public override async Task<ApiEmployeeportal.BatchGetPortalHomesResponse> BatchGetPortalHomes(ApiEmployeeportal.BatchGetPortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "BatchGetPortalHomes", "accepted", token);
        var input0 = new ApiDocuments.PushDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentTemplateRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Metadata = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.PushDocumentTemplateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.TestSelfServiceRequestConnectionRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.TestSelfServiceRequestConnectionRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Result?.Message ?? ""),
                ReceivedAt = (reply0.DocumentTemplate?.IssuedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _employeeportalSelfServiceRequestIntegrationService.TestSelfServiceRequestConnectionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.BatchGetPortalHomesResponse
        {
            PortalHomes = { new ApiEmployeeportal.PortalHome
            {
                Comments = { new ApiEmployeeportal.PortalHomeComment
                {
                    CreatedAt = (reply0.DocumentTemplate?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Sla = new ApiEmployeeportal.PortalHomeSla
                {
                    DueAt = (reply1.SelfServiceRequest?.Sla?.DueAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Creates a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.ClonePortalHomeResponse> ClonePortalHome(ApiEmployeeportal.ClonePortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ClonePortalHome for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.PortalHome?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.ResolveDocumentTemplateConflictRequest>("acme.employeeportal.v1.PortalHomeService/ClonePortalHome/0", input0.Clone()), token);
        var input1 = new ApiEmployeeportal.SignalAnnouncementWorkflowRequest
        {
            Announcement = new ApiEmployeeportal.Announcement
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply0.DocumentTemplate?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SignalAnnouncementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.PortalHome?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiEmployeeportal.SignalAnnouncementWorkflowRequest>(json1);
        using var rpc1 = _employeeportalAnnouncementWorkflowService.SignalAnnouncementWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ListPendingPositionsRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Filter = new ApiEmployees.ListPendingPositionsRequest.Types.Filter
        {
            Region = (reply0.DocumentTemplate?.BillingAddress?.Region ?? ""),
        };
        using var rpc2 = _employeesPositionApprovalService.ListPendingPositionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployeeportal.ConnectAnnouncementSourceRequest
        {
            AnnouncementId = (request.PortalHome ?? new ApiEmployeeportal.PortalHome()).OriginCase switch
            {
                ApiEmployeeportal.PortalHome.OriginOneofCase.Customer => (request.PortalHome?.Customer?.AnnouncementId ?? ""),
                _ => ""
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectAnnouncementSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc3 = _employeeportalAnnouncementIntegrationService.ConnectAnnouncementSourceAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.ClonePortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Announcement?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                PortalHomeId = (request.PortalHome?.PortalHomeId ?? ""),
                Priority = (reply3.Announcement?.Counterparty?.Priority ?? 0),
                Requester = (request.PortalHome?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ClonePortalHome", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.CreatePortalHomeResponse> CreatePortalHome(ApiEmployeeportal.CreatePortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreatePortalHome for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PauseDocumentTemplateWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.PortalHome?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateWorkflowService.PauseDocumentTemplateWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.StartPositionWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesPositionWorkflowService.StartPositionWorkflowAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployees.ListPositionWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListPositionWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (reply0.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.PortalHome?.Assignee?.UserId ?? ""),
                Region = (reply0.DocumentTemplate?.BillingAddress?.Region ?? ""),
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _employeesPositionWorkflowService.ListPositionWorkflowsAsync);
        var response = new ApiEmployeeportal.CreatePortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply1.Position?.Owner?.UserId ?? ""),
                },
                PortalHomeId = (request.PortalHome?.PortalHomeId ?? ""),
                Priority = (request.PortalHome?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "CreatePortalHome", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.DeletePortalHomeResponse> DeletePortalHome(ApiEmployeeportal.DeletePortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PortalHomeId, "portal_home_id");
        _logger.LogInformation("Handling DeletePortalHome for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "DeletePortalHome", "accepted", token);
        var input0 = new ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ConnectSelfServiceRequestSourceRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _employeeportalSelfServiceRequestIntegrationService.ConnectSelfServiceRequestSourceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.RepairSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RepairSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    ObservedAt = (reply0.SelfServiceRequest?.Sla?.DueAt ?? new Timestamp()),
                } },
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.SelfServiceRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureAdminService.RepairSignatureAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.SubscribeAnnouncementUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.SelfServiceRequest?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _employeeportalAnnouncementNotificationService.SubscribeAnnouncementUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.DeletePortalHomeResponse
        {
            EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.PortalHomeId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "DeletePortalHome", (request.PortalHomeId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.GetPortalHomeResponse> GetPortalHome(ApiEmployeeportal.GetPortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PortalHomeId, "portal_home_id");
        _logger.LogInformation("Handling GetPortalHome for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "GetPortalHome", "accepted", token);
        var response = new ApiEmployeeportal.GetPortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListPortalHomesResponse> ListPortalHomes(ApiEmployeeportal.ListPortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "ListPortalHomes", "accepted", token);
        var input0 = new ApiDocuments.ReconcileDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc0 = _documentsDocumentSyncService.ReconcileDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.ListPortalHomesResponse
        {
            PortalHomes = { new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (reply0.Document?.Audit?.UpdatedBy ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ListPortalHomes", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.SearchPortalHomesResponse> SearchPortalHomes(ApiEmployeeportal.SearchPortalHomesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SearchPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "SearchPortalHomes", "accepted", token);
        var response = new ApiEmployeeportal.SearchPortalHomesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "SearchPortalHomes", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.UpdatePortalHomeResponse> UpdatePortalHome(ApiEmployeeportal.UpdatePortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UpdatePortalHome for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployeeportal.StreamSelfServiceRequestNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.PortalHome?.Requester?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.PortalHome?.Requester?.Phone ?? ""),
            },
        };
        using var stream0 = _employeeportalSelfServiceRequestNotificationService.StreamSelfServiceRequestNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamSelfServiceRequestNotices frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiAuthorization.ExplainAccessPolicyDecisionRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.PortalHome?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.PortalHome?.Assignee?.DisplayName ?? ""),
                    UserId = (request.PortalHome?.Assignee?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.ExplainAccessPolicyDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.PortalHome?.Requester?.Email ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _authorizationAccessPolicyValidationService.ExplainAccessPolicyDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployeeportal.AcknowledgeAnnouncementEventRequest
        {
            AnnouncementId = (request.PortalHome?.Customer?.AnnouncementId ?? ""),
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgeAnnouncementEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                ReceivedAt = (reply1.AccessPolicy?.Audit?.CreatedAt ?? new Timestamp()),
            },
            Reason = (reply1.AccessPolicy?.Block?.Reason ?? ""),
        };
        using var rpc2 = _employeeportalAnnouncementIntegrationService.AcknowledgeAnnouncementEventAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ApproveSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.AccessPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc3 = _licensingSoftwareLicenseApprovalService.ApproveSoftwareLicenseAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiEmployeeportal.UpdatePortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.SoftwareLicense?.Holder?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.AccessPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                PortalHomeId = (request.PortalHome?.PortalHomeId ?? ""),
                Priority = (reply2.Announcement?.Counterparty?.Priority ?? 0),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.AccessPolicy?.Owner?.Email ?? ""),
                },
                Subject = (request.PortalHome?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.PortalHome?.Sla?.PolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "UpdatePortalHome", (request.PortalHome?.Sla?.PolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of portal homes as they happen.
    /// </summary>
    public override async Task WatchPortalHomes(ApiEmployeeportal.WatchPortalHomesRequest request, IServerStreamWriter<ApiEmployeeportal.WatchPortalHomesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling WatchPortalHomes for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "WatchPortalHomes", "accepted", token);
        var input0 = new ApiDocuments.ListSignatureExportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureExportsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _documentsSignatureExportService.ListSignatureExportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.AcknowledgeAnnouncementEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgeAnnouncementEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Result?.Message ?? ""),
                ReceivedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
            },
        };
        using var rpc1 = _employeeportalAnnouncementIntegrationService.AcknowledgeAnnouncementEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.WatchPortalHomesResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "WatchPortalHomes", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
