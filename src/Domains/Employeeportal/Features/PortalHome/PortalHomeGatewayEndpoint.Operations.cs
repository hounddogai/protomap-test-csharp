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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDriverapp = Acme.Driverapp.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class PortalHomeGatewayEndpoint
{
    /// <summary>
    /// Composes a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.ComposePortalHomePageResponse> ComposePortalHomePage(ApiEmployeeportal.ComposePortalHomePageRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ComposePortalHomePage for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.CancelFeedbackWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.CancelFeedbackWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.PortalHome?.Assignee?.Email ?? ""),
                },
            },
            Feedback = new ApiPerformance.Feedback
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _performanceFeedbackWorkflowService.CancelFeedbackWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ApproveVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.PortalHome?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.PortalHome?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.PortalHome?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.PortalHome?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        var reply1 = await SendComposePortalHomePage1Async(input1, token);
        var input2 = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.UnsubscribeAnnouncementUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _employeeportalAnnouncementNotificationService.UnsubscribeAnnouncementUpdatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployeeportal.ComposePortalHomePageResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = (request.PortalHome?.Assignee ?? new ApiIdentity.UserRef()),
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.Announcement?.Audit?.UpdatedBy ?? ""),
                },
                Comments = { new ApiEmployeeportal.PortalHomeComment
                {
                    CreatedAt = (reply1.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ComposePortalHomePage", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Fetchs a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.FetchPortalHomeViewResponse> FetchPortalHomeView(ApiEmployeeportal.FetchPortalHomeViewRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FetchPortalHomeView for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployeeportal.SummarizeSelfServiceRequestsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployeeportal.SummarizeSelfServiceRequestsRequest>();
        using var rpc0 = _employeeportalSelfServiceRequestQueryService.SummarizeSelfServiceRequestsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiDriverapp.CreateDriverHomeResponse();
        foreach (var item in (request.PortalHome?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.PortalHomeStatusChange>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiDriverapp.CreateDriverHomeRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DriverHome = new ApiDriverapp.DriverHome
                {
                    History = { new ApiDriverapp.DriverHomeStatusChange
                    {
                        ChangedAt = (item.ChangedAt ?? new Timestamp()),
                        ChangedBy = (item.ChangedBy ?? ""),
                        FromStatus = (item.FromStatus ?? ""),
                        ToStatus = (item.ToStatus ?? ""),
                    } },
                },
            };
            using var rpc1 = _driverappDriverHomeService.CreateDriverHomeAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var input2 = new ApiEmployeeportal.ConnectAnnouncementSourceRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
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
            Reason = (request.Reason ?? ""),
        };
        using var rpc2 = _employeeportalAnnouncementIntegrationService.ConnectAnnouncementSourceAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployeeportal.ReplaySelfServiceRequestEventsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.ReplaySelfServiceRequestEventsRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply2.Result?.Message ?? ""),
                ReceivedAt = (reply1.DriverHome?.Audit?.CreatedAt ?? new Timestamp()),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc3 = _employeeportalSelfServiceRequestIntegrationService.ReplaySelfServiceRequestEventsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.FetchPortalHomeViewResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply3.SelfServiceRequest?.Requester?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply3.SelfServiceRequest?.Audit?.UpdatedBy ?? ""),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
                Priority = (reply2.Announcement?.Counterparty?.Priority ?? 0),
                Requester = (request.PortalHome?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.PortalHome?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "FetchPortalHomeView", (request.PortalHome?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.GetPortalHomeSummaryResponse> GetPortalHomeSummary(ApiEmployeeportal.GetPortalHomeSummaryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.PortalHomeId, "portal_home_id");
        _logger.LogInformation("Handling GetPortalHomeSummary for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "GetPortalHomeSummary", "accepted", token);
        var input0 = new ApiScheduling.CancelShiftWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Shift = new ApiScheduling.Shift
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiScheduling.ShiftParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                    ParticipantId = (request.PortalHomeId ?? ""),
                } },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _schedulingShiftWorkflowService.CancelShiftWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiEmployeeportal.GetPortalHomeSummaryResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Shift?.Audit?.UpdatedBy ?? ""),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Refreshs a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.RefreshPortalHomeResponse> RefreshPortalHome(ApiEmployeeportal.RefreshPortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RefreshPortalHome for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.PortalHome?.History ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.PortalHomeStatusChange>()).Select(async item =>
        {
            var input0 = new ApiEmployeeportal.AdvanceAnnouncementWorkflowRequest
            {
                Announcement = new ApiEmployeeportal.Announcement
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (item.ChangedAt ?? new Timestamp()),
                    },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc0 = _employeeportalAnnouncementWorkflowService.AdvanceAnnouncementWorkflowAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiEmployeeportal.AdvanceAnnouncementWorkflowResponse();
        var reply1 = new ApiDocuments.PreviewDocumentFolderResponse();
        foreach (var item in (request.PortalHome?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.PortalHomeComment>()))
        {
            var input1 = new ApiDocuments.PreviewDocumentFolderRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                DocumentFolder = new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Children = { new ApiDocuments.DocumentFolder
                        {
                            Audit = new ApiCommon.AuditInfo
                            {
                                CreatedAt = (item.CreatedAt ?? new Timestamp()),
                            },
                        } },
                    } },
                },
            };
            using var rpc1 = _documentsDocumentFolderValidationService.PreviewDocumentFolderAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiEmployeeportal.RefreshPortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
                Status = (request.PortalHome?.Status ?? (ApiEmployeeportal.PortalHomeStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "RefreshPortalHome", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.SubmitPortalHomeFormResponse> SubmitPortalHomeForm(ApiEmployeeportal.SubmitPortalHomeFormRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitPortalHomeForm for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.UnsubscribePortalHomeUpdatesRequest.Types.Details
            {
                Channel = (request.PortalHome ?? new ApiEmployeeportal.PortalHome()).OriginCase switch
                {
                    ApiEmployeeportal.PortalHome.OriginOneofCase.Channel => (request.PortalHome?.Channel?.Channel ?? ""),
                    _ => ""
                },
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.PortalHome?.Assignee?.Email ?? ""),
                    Name = (request.PortalHome?.Requester?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.PortalHome?.Requester?.Phone ?? ""),
                },
            },
        };
        using var rpc0 = _employeeportalPortalHomeNotificationService.UnsubscribePortalHomeUpdatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployeeportal.AcknowledgeSelfServiceRequestEventRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployeeportal.AcknowledgeSelfServiceRequestEventRequest.Types.Details
            {
                Envelope = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                MessageId = (reply0.Result?.Message ?? ""),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _employeeportalSelfServiceRequestIntegrationService.AcknowledgeSelfServiceRequestEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.SuggestDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.SuggestDocumentFoldersRequest.Types.Filter
            {
                CreatedAfter = (request.PortalHome?.Sla?.DueAt ?? new Timestamp()),
                Labels = { (request.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.PortalHome ?? new ApiEmployeeportal.PortalHome()).OriginCase switch
                {
                    ApiEmployeeportal.PortalHome.OriginOneofCase.Customer => (reply0.PortalHome?.Customer?.AnnouncementId ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _documentsDocumentFolderQueryService.SuggestDocumentFoldersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiDocuments.ListDocumentTemplateVersionsResponse();
        foreach (var item in (request.PortalHome?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.PortalHomeComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiDocuments.ListDocumentTemplateVersionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListDocumentTemplateVersionsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc3 = _documentsDocumentTemplateHistoryService.ListDocumentTemplateVersionsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiEmployeeportal.SubmitPortalHomeFormResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.PortalHome?.Requester?.Email ?? ""),
                },
                CustomFields = { (reply0.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                PortalHomeId = (request.PortalHomeId ?? ""),
                Priority = (request.PortalHome?.Priority ?? 0),
                Requester = (reply1.SelfServiceRequest?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.PortalHome?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "SubmitPortalHomeForm", (request.PortalHome?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Tracks a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.TrackPortalHomeResponse> TrackPortalHome(ApiEmployeeportal.TrackPortalHomeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling TrackPortalHome for {Tenant}", requestContext.TenantId);
        if (request.PortalHome is { } record)
        {
            var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.WatchDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.PortalHome?.Assignee?.Email ?? ""),
            },
        };
        using var stream0 = _documentsDocumentService.WatchDocuments(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchDocuments frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiItsm.ListChangeRequestWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListChangeRequestWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.PortalHome?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.PortalHome?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.PortalHome?.Assignee?.UserId ?? ""),
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmChangeRequestWorkflowService.ListChangeRequestWorkflowsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.TrackPortalHomeResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
                Priority = (request.PortalHome?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "TrackPortalHome", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Takes a stream of portal homes and answers once at the end.
    /// </summary>
    public override async Task<ApiEmployeeportal.UploadPortalHomeAttachmentsResponse> UploadPortalHomeAttachments(IAsyncStreamReader<ApiEmployeeportal.UploadPortalHomeAttachmentsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiEmployeeportal.UploadPortalHomeAttachmentsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling UploadPortalHomeAttachments for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiDocuments.RecordSignatureVersionRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
                {
                    Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                },
                Signature = new ApiDocuments.Signature
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Subject = new ApiDocuments.DocumentRef
                    {
                        DisplayName = request.ChunkCase switch
                        {
                            ApiEmployeeportal.UploadPortalHomeAttachmentsRequest.ChunkOneofCase.Record => (request.Record?.Assignee?.DisplayName ?? ""),
                            _ => ""
                        },
                        Number = request.ChunkCase switch
                        {
                            ApiEmployeeportal.UploadPortalHomeAttachmentsRequest.ChunkOneofCase.Record => (request.Record?.Customer?.Number ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiCorporateevents.CommentVenueApprovalRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiCorporateevents.CommentVenueApprovalRequest.Types.Details
                {
                    Approver = new ApiIdentity.UserRef
                    {
                        Email = request.ChunkCase switch
                        {
                            ApiEmployeeportal.UploadPortalHomeAttachmentsRequest.ChunkOneofCase.Record => (request.Record?.Requester?.Email ?? ""),
                            _ => ""
                        },
                    },
                },
                Venue = new ApiCorporateevents.Venue
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = request.ChunkCase switch
                        {
                            ApiEmployeeportal.UploadPortalHomeAttachmentsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.Etag ?? ""),
                            _ => ""
                        },
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = request.ChunkCase switch
                        {
                            ApiEmployeeportal.UploadPortalHomeAttachmentsRequest.ChunkOneofCase.Record => (request.Record?.Customer?.DisplayName ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc1 = _corporateeventsVenueApprovalService.CommentVenueApprovalAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = new ApiEmployeeportal.BrowseSelfServiceRequestsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiEmployeeportal.BrowseSelfServiceRequestsRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiEmployeeportal.UploadPortalHomeAttachmentsRequest.ChunkOneofCase.Record => (request.Record?.Sla?.DueAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    Region = (reply1.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply1.Venue?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc2 = _employeeportalSelfServiceRequestQueryService.BrowseSelfServiceRequestsAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiEmployeeportal.UploadPortalHomeAttachmentsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Subject ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "portal_home",
                "audit", "UploadPortalHomeAttachments", (request.Record?.Subject ?? ""), token);
            result = response;
        }
        return result;
    }
    private async Task<ApiCorporateevents.ApproveVenueResponse> SendComposePortalHomePage1Async(ApiCorporateevents.ApproveVenueRequest request, CancellationToken token)
    {
        return await _portalHomeGatewayEndpointSendComposePortalHomePage1Port.SendAsync(request, token);
    }
}
