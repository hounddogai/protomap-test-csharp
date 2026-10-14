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
using ApiDatahub = Acme.Datahub.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEdgegateway = Acme.Edgegateway.V1;
using ApiEmployeeportal = Acme.Employeeportal.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFanportal = Acme.Fanportal.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLearning = Acme.Learning.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;

namespace Acme.Workforce.Employeeportal;

public sealed partial class PortalHomeIntegrationEndpoint
{
    /// <summary>
    /// Acknowledges a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.AcknowledgePortalHomeEventResponse> AcknowledgePortalHomeEvent(ApiEmployeeportal.AcknowledgePortalHomeEventRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AcknowledgePortalHomeEvent for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "AcknowledgePortalHomeEvent", "accepted", token);
        var input0 = new ApiDocuments.RepairSignatureRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RepairSignatureRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Details?.ReceivedAt ?? new Timestamp()),
            },
        };
        using var rpc0 = _documentsSignatureAdminService.RepairSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.PauseManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiDevices.PauseManagedDeviceWorkflowRequest>(json1);
        using var rpc1 = _devicesManagedDeviceWorkflowService.PauseManagedDeviceWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.AcknowledgePortalHomeEventResponse
        {
            Outcome = new ApiEmployeeportal.AcknowledgePortalHomeEventResponse.Types.Outcome
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ManagedDevice?.Audit?.Etag ?? ""),
                },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "AcknowledgePortalHomeEvent", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Connects a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.ConnectPortalHomeSourceResponse> ConnectPortalHomeSource(ApiEmployeeportal.ConnectPortalHomeSourceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ConnectPortalHomeSource for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "ConnectPortalHomeSource", "accepted", token);
        var input0 = new ApiRecruiting.StreamOfferMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var stream0 = _recruitingOfferReportingService.StreamOfferMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamOfferMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiFanportal.UpdateFanSessionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            FanSession = new ApiFanportal.FanSession
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _fanportalFanSessionService.UpdateFanSessionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.ConnectPortalHomeSourceResponse
        {
            Outcome = new ApiEmployeeportal.ConnectPortalHomeSourceResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ConnectPortalHomeSource", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Disconnects a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.DisconnectPortalHomeSourceResponse> DisconnectPortalHomeSource(ApiEmployeeportal.DisconnectPortalHomeSourceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DisconnectPortalHomeSource for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "DisconnectPortalHomeSource", "accepted", token);
        var input0 = new ApiRecruiting.PauseCandidateWorkflowRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Candidate = new ApiRecruiting.Candidate
            {
                Passport = new ApiRecruiting.CandidatePassport
                {
                    ExpiresAt = (request.Details?.ReceivedAt ?? new Timestamp()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.PauseCandidateWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateWorkflowService.PauseCandidateWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.ListPendingEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListPendingEmploymentContractsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Candidate?.Passport?.ExpiresAt ?? new Timestamp()),
                Region = (reply0.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _employeesEmploymentContractApprovalService.ListPendingEmploymentContractsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ListSignatureVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListSignatureVersionsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Candidate?.Passport?.ExpiresAt ?? new Timestamp()),
                Region = (reply0.Candidate?.PrimaryAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _documentsSignatureHistoryService.ListSignatureVersionsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiEdgegateway.CreateRouteBindingRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RouteBinding = new ApiEdgegateway.RouteBinding
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Candidate?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc3 = _sendDisconnectPortalHomeSource3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.DisconnectPortalHomeSourceResponse
        {
            Outcome = new ApiEmployeeportal.DisconnectPortalHomeSourceResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "DisconnectPortalHomeSource", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Takes a stream of portal homes and answers once at the end.
    /// </summary>
    public override async Task<ApiEmployeeportal.IngestPortalHomeEventsResponse> IngestPortalHomeEvents(IAsyncStreamReader<ApiEmployeeportal.IngestPortalHomeEventsRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiEmployeeportal.IngestPortalHomeEventsResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling IngestPortalHomeEvents for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiAuthorization.ListAccessPoliciesRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiAuthorization.ListAccessPoliciesRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiEmployeeportal.IngestPortalHomeEventsRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    Labels = { request.ChunkCase switch
                    {
                        ApiEmployeeportal.IngestPortalHomeEventsRequest.ChunkOneofCase.Record => (request.Record?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()),
                        _ => new Google.Protobuf.Collections.MapField<string, string>()
                    } },
                    OwnerId = request.ChunkCase switch
                    {
                        ApiEmployeeportal.IngestPortalHomeEventsRequest.ChunkOneofCase.Record => (request.Record?.Customer?.AnnouncementId ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc0 = _authorizationAccessPolicyService.ListAccessPoliciesAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiDocuments.ListSignatureVersionsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiDocuments.ListSignatureVersionsRequest.Types.Filter
                {
                    CreatedAfter = request.ChunkCase switch
                    {
                        ApiEmployeeportal.IngestPortalHomeEventsRequest.ChunkOneofCase.Record => (request.Record?.Sla?.DueAt ?? new Timestamp()),
                        _ => new Timestamp()
                    },
                    OwnerId = request.ChunkCase switch
                    {
                        ApiEmployeeportal.IngestPortalHomeEventsRequest.ChunkOneofCase.Record => (request.Record?.Sla?.PolicyId ?? ""),
                        _ => ""
                    },
                },
            };
            using var rpc1 = _documentsSignatureHistoryService.ListSignatureVersionsAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var response = new ApiEmployeeportal.IngestPortalHomeEventsResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.Subject ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "portal_home",
                "audit", "IngestPortalHomeEvents", (request.Record?.Subject ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Lists portal homes a page at a time.
    /// </summary>
    public override async Task<ApiEmployeeportal.ListPortalHomeSourcesResponse> ListPortalHomeSources(ApiEmployeeportal.ListPortalHomeSourcesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPortalHomeSources for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", "batch",
            "ListPortalHomeSources", "accepted", token);
        var input0 = new ApiEdgegateway.ListRouteBindingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEdgegateway.ListRouteBindingsRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiEdgegateway.ListRouteBindingsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _edgegatewayRouteBindingService.ListRouteBindingsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployeeportal.ListPortalHomeSourcesResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply0.PageInfo?.NextPageToken ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ListPortalHomeSources", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Exchanges changes of portal homes with a peer.
    /// </summary>
    public override async Task RelayPortalHomeMessages(IAsyncStreamReader<ApiEmployeeportal.RelayPortalHomeMessagesRequest> requests, IServerStreamWriter<ApiEmployeeportal.RelayPortalHomeMessagesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            _logger.LogInformation("Handling RelayPortalHomeMessages for {Tenant}", requestContext.TenantId);
            if (request.Change is { } record)
            {
                var snapshot = PortalHomeMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var reply0 = new ApiDocuments.RecordSignatureVersionResponse();
            foreach (var item in (request.Change?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiEmployeeportal.PortalHomeComment>()))
            {
                var input0 = new ApiDocuments.RecordSignatureVersionRequest
                {
                    Context = (request.Context ?? new ApiCommon.RequestContext()),
                    Details = new ApiDocuments.RecordSignatureVersionRequest.Types.Details
                    {
                        AuthorEmail = (item.AuthorEmail ?? ""),
                        Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
                    },
                    Signature = new ApiDocuments.Signature
                    {
                        Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    },
                };
                using var rpc0 = _documentsSignatureHistoryService.RecordSignatureVersionAsync(input0, cancellationToken: token);
                reply0 = await rpc0.ResponseAsync;
            }
            var input1 = new ApiLearning.RejectCourseEnrollmentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                CourseEnrollment = new ApiLearning.CourseEnrollment
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelayPortalHomeMessagesRequest.FrameOneofCase.Change => (request.Change?.Audit?.CreatedBy ?? ""),
                            _ => ""
                        },
                    },
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelayPortalHomeMessagesRequest.FrameOneofCase.Change => (request.Change?.Requester?.PreferredChannel ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc1 = _learningCourseEnrollmentApprovalService.RejectCourseEnrollmentAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = new ApiScheduling.CancelRosterWorkflowRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.CancelRosterWorkflowRequest.Types.Details
                {
                    Notify = new ApiCommon.ContactInfo
                    {
                        Email = (reply1.CourseEnrollment?.Contact?.Email ?? ""),
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        Etag = request.FrameCase switch
                        {
                            ApiEmployeeportal.RelayPortalHomeMessagesRequest.FrameOneofCase.Change => (request.Change?.Audit?.Etag ?? ""),
                            _ => ""
                        },
                        UpdatedBy = (reply1.CourseEnrollment?.Audit?.UpdatedBy ?? ""),
                    },
                    Card = new ApiScheduling.RosterCardSettlement
                    {
                        Last4 = (reply1.CourseEnrollment ?? new ApiLearning.CourseEnrollment()).PaymentCase switch
                        {
                            ApiLearning.CourseEnrollment.FundingOneofCase.Card => (reply1.CourseEnrollment?.Card?.Last4 ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc2 = _schedulingRosterWorkflowService.CancelRosterWorkflowAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var response = new ApiEmployeeportal.RelayPortalHomeMessagesResponse
            {
                PortalHome = new ApiEmployeeportal.PortalHome
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedBy = (reply1.CourseEnrollment?.Audit?.CreatedBy ?? ""),
                    },
                    CustomFields = { (request.Change?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Requester = (request.Change?.Requester ?? new ApiCommon.ContactInfo()),
                },
                Result = new ApiCommon.OperationResult
                {
                    Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Cursor ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "portal_home",
                "audit", "RelayPortalHomeMessages", (request.Cursor ?? ""), token);
            await responses.WriteAsync(response, token);
        }
    }
    /// <summary>
    /// Replays a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.ReplayPortalHomeEventsResponse> ReplayPortalHomeEvents(ApiEmployeeportal.ReplayPortalHomeEventsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ReplayPortalHomeEvents for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "ReplayPortalHomeEvents", "accepted", token);
        var input0 = new ApiDatahub.SuggestWorkspacesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDatahub.SuggestWorkspacesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.ReceivedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.MessageId ?? ""),
            },
        };
        var reply0 = await SendReplayPortalHomeEvents0Async(input0, token);
        var input1 = new ApiItsm.ScheduleChangeRequestReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScheduleChangeRequestReportRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _itsmChangeRequestReportingService.ScheduleChangeRequestReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployeeportal.ReplayPortalHomeEventsResponse
        {
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "ReplayPortalHomeEvents", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    /// <summary>
    /// Tests a portal home.
    /// </summary>
    public override async Task<ApiEmployeeportal.TestPortalHomeConnectionResponse> TestPortalHomeConnection(ApiEmployeeportal.TestPortalHomeConnectionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling TestPortalHomeConnection for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "portal_home", request.PortalHomeId,
            "TestPortalHomeConnection", "accepted", token);
        var input0 = new ApiFeatureflags.SuggestFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.SuggestFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.ReceivedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.MessageId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiFeatureflags.SuggestFeatureFlagsRequest>();
        using var rpc0 = _featureflagsFeatureFlagQueryService.SuggestFeatureFlagsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.PushDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentSyncService.PushDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.ListOfferReportsRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Filter = new ApiRecruiting.ListOfferReportsRequest.Types.Filter
        {
            Region = (reply1.Document?.BillingAddress?.Region ?? ""),
        };
        using var rpc2 = _recruitingOfferReportingService.ListOfferReportsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiNotifications.CreateNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Document?.Counterparty?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Document?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply1.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    Number = (reply1.Document?.Number ?? ""),
                },
            },
        };
        using var rpc3 = _notificationsNotificationService.CreateNotificationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiEmployeeportal.TestPortalHomeConnectionResponse
        {
            Outcome = new ApiEmployeeportal.TestPortalHomeConnectionResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            PortalHome = new ApiEmployeeportal.PortalHome
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Document?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiEmployeeportal.PortalHomeComment
                {
                    CreatedAt = (reply3.Notification?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                PortalHomeId = (request.PortalHomeId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.SourceSystem ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "portal_home",
            "audit", "TestPortalHomeConnection", (request.Details?.SourceSystem ?? ""), token);
        return response;
    }
    private async Task<ApiDatahub.SuggestWorkspacesResponse> SendReplayPortalHomeEvents0Async(ApiDatahub.SuggestWorkspacesRequest request, CancellationToken token)
    {
        return await _portalHomeIntegrationEndpointSendReplayPortalHomeEvents0Port.SendAsync(request, token);
    }
}
