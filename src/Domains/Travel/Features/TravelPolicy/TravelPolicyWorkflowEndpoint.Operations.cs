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
using ApiBroadcasts = Acme.Broadcasts.V1;
using ApiCart = Acme.Cart.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Travel;

public sealed partial class TravelPolicyWorkflowEndpoint
{
    /// <summary>
    /// Advances a travel policy.
    /// </summary>
    public override async Task<ApiTravel.AdvanceTravelPolicyWorkflowResponse> AdvanceTravelPolicyWorkflow(ApiTravel.AdvanceTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling AdvanceTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeEmployeesRequest.Types.Details
            {
                Period = (request.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _employeesEmployeeQueryService.SummarizeEmployeesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiBroadcasts.ListBroadcastsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBroadcasts.ListBroadcastsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _broadcastsBroadcastService.ListBroadcastsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiExperiments.FindExperimentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiExperiments.FindExperimentsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelPolicy?.Owner?.UserId ?? ""),
            },
        };
        using var rpc2 = _experimentsExperimentQueryService.FindExperimentsAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiTravel.AdvanceTravelPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = (request.TravelPolicy?.Owner ?? new ApiIdentity.UserRef()),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "AdvanceTravelPolicyWorkflow", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a travel policy.
    /// </summary>
    public override async Task<ApiTravel.CancelTravelPolicyWorkflowResponse> CancelTravelPolicyWorkflow(ApiTravel.CancelTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CancelTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ShareTravelBookingReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ShareTravelBookingReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _travelTravelBookingReportingService.ShareTravelBookingReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncSignaturesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncSignaturesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                    Number = (reply0.TravelBooking?.Number ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureSyncService.SyncSignaturesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDocuments.SyncSignaturesRequest>("acme.travel.v1.TravelPolicyWorkflowService/CancelTravelPolicyWorkflow/1", input1.Clone()), token);
        var response = new ApiTravel.CancelTravelPolicyWorkflowResponse
        {
            Outcome = new ApiTravel.CancelTravelPolicyWorkflowResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Outcome?.Status ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.TravelBooking?.Counterparty?.Email ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
                Version = (request.TravelPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "CancelTravelPolicyWorkflow", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a travel policy.
    /// </summary>
    public override async Task<ApiTravel.InspectTravelPolicyWorkflowResponse> InspectTravelPolicyWorkflow(ApiTravel.InspectTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TravelPolicyId, "travel_policy_id");
        _logger.LogInformation("Handling InspectTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", request.TravelPolicyId,
            "InspectTravelPolicyWorkflow", "accepted", token);
        var input0 = new ApiDocuments.RetryDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RetryDocumentExportRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _documentsDocumentExportService.RetryDocumentExportAsync);
        var response = new ApiTravel.InspectTravelPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Document?.Audit?.Etag ?? ""),
                },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists travel policies a page at a time.
    /// </summary>
    public override async Task<ApiTravel.ListTravelPolicyWorkflowsResponse> ListTravelPolicyWorkflows(ApiTravel.ListTravelPolicyWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListTravelPolicyWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy", "batch",
            "ListTravelPolicyWorkflows", "accepted", token);
        var input0 = new ApiTravel.CommentTravelPolicyApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.CommentTravelPolicyApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendListTravelPolicyWorkflows0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.ListTravelPolicyWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ListTravelPolicyWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a travel policy.
    /// </summary>
    public override async Task<ApiTravel.PauseTravelPolicyWorkflowResponse> PauseTravelPolicyWorkflow(ApiTravel.PauseTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PauseTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.LintDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.TravelPolicy?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        var named0 = _registry.Get<ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient>("acme.documents.v1.DocumentFolderValidationService");
        using var rpc0 = named0.LintDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
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
                            Etag = (request.TravelPolicy?.Audit?.Etag ?? ""),
                            UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (reply0.DocumentFolder?.Location?.Recipient ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderValidationService.PreviewDocumentFolderAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiTravel.ShareTravelBookingReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.ShareTravelBookingReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _travelTravelBookingReportingService.ShareTravelBookingReportAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiTravel.PauseTravelPolicyWorkflowResponse
        {
            Outcome = new ApiTravel.PauseTravelPolicyWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Name = (reply0.DocumentFolder?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.TravelBooking?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.DocumentFolder?.Manager?.Email ?? ""),
                },
                Status = (request.TravelPolicy?.Status ?? (ApiTravel.TravelPolicyStatus)0),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "PauseTravelPolicyWorkflow", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a travel policy.
    /// </summary>
    public override async Task<ApiTravel.ResumeTravelPolicyWorkflowResponse> ResumeTravelPolicyWorkflow(ApiTravel.ResumeTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResumeTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.DeleteTravelPolicyReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Etag = (request.TravelPolicy?.Audit?.Etag ?? ""),
            Reason = (request.TravelPolicy ?? new ApiTravel.TravelPolicy()).ActionCase switch
            {
                ApiTravel.TravelPolicy.ActionOneofCase.Block => (request.TravelPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
            TravelPolicyId = (request.TravelPolicy?.TravelPolicyId ?? ""),
        };
        using var rpc0 = _travelTravelPolicyReportingService.DeleteTravelPolicyReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.CloneNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.TravelPolicy?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.TravelPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _notificationsNotificationService.CloneNotificationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.ResumeTravelPolicyWorkflowToBatchGetCarts2();
        using var rpc2 = _cartCartService.BatchGetCartsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiTravel.ResumeTravelPolicyWorkflowResponse
        {
            Outcome = new ApiTravel.ResumeTravelPolicyWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply0.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = (request.TravelPolicy?.Owner ?? new ApiIdentity.UserRef()),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "ResumeTravelPolicyWorkflow", (request.TravelPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a travel policy.
    /// </summary>
    public override async Task<ApiTravel.RetryTravelPolicyWorkflowResponse> RetryTravelPolicyWorkflow(ApiTravel.RetryTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RetryTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.StreamItineraryMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.Notify?.Phone ?? ""),
                PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _travelItineraryReportingService.StreamItineraryMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamItineraryMetrics frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiEmployees.FindOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.TravelPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.TravelPolicy?.TravelPolicyId ?? ""),
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeesOrgUnitQueryService.FindOrgUnitsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.RetryTravelPolicyWorkflowResponse
        {
            Outcome = new ApiTravel.RetryTravelPolicyWorkflowResponse.Types.Outcome
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Parameters = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "RetryTravelPolicyWorkflow", (request.TravelPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a travel policy.
    /// </summary>
    public override async Task<ApiTravel.SignalTravelPolicyWorkflowResponse> SignalTravelPolicyWorkflow(ApiTravel.SignalTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SignalTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CommentEscalationApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CommentEscalationApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Escalation = new ApiHelpdesk.Escalation
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.TravelPolicy?.Owner?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiHelpdesk.CommentEscalationApprovalRequest>();
        using var rpc0 = _helpdeskEscalationApprovalService.CommentEscalationApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.StartVenueWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.StartVenueWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Escalation?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiCorporateevents.StartVenueWorkflowRequest>(json1);
        using var rpc1 = _corporateeventsVenueWorkflowService.StartVenueWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.SignalTravelPolicyWorkflowResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Name = (request.TravelPolicy?.Name ?? ""),
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.TravelPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "SignalTravelPolicyWorkflow", (request.TravelPolicy?.TravelPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a travel policy.
    /// </summary>
    public override async Task<ApiTravel.StartTravelPolicyWorkflowResponse> StartTravelPolicyWorkflow(ApiTravel.StartTravelPolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StartTravelPolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.TravelPolicy is { } record)
        {
            var snapshot = TravelPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTravel.ReinstateTravelPolicyRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiTravel.ReinstateTravelPolicyRequest.Types.Details
        {
            NewOwner = new ApiIdentity.UserRef
            {
                DisplayName = (request.TravelPolicy?.Owner?.DisplayName ?? ""),
                Email = (request.Details?.Notify?.Email ?? ""),
            },
            NotifyContact = new ApiCommon.ContactInfo
            {
                PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
            },
        };
        input0.TravelPolicyId = (request.TravelPolicy?.TravelPolicyId ?? "");
        using var rpc0 = _travelTravelPolicyLifecycleService.ReinstateTravelPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.StartTravelPolicyWorkflowResponse
        {
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.TravelPolicy?.Audit?.Etag ?? ""),
                },
                Effective = (reply0.TravelPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                Parameters = { (request.TravelPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TravelPolicyId = (request.TravelPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.TravelPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "travel_policy",
            "audit", "StartTravelPolicyWorkflow", (request.TravelPolicy?.Route?.Target ?? ""), token);
        return response;
    }
}
