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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDeliveries = Acme.Deliveries.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRisk = Acme.Risk.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class RegistrationReportingEndpoint
{
    /// <summary>
    /// Deletes a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.DeleteRegistrationReportResponse> DeleteRegistrationReport(ApiCorporateevents.DeleteRegistrationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling DeleteRegistrationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "DeleteRegistrationReport", "accepted", token);
        var input0 = new ApiCorporateevents.ResolveVenueConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ResolveVenueConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Reason = (request.Reason ?? ""),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsVenueSyncService.ResolveVenueConflictAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.LintDocumentFoldersRequest
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
                            Etag = (reply0.Venue?.Audit?.Etag ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (reply0.Venue?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _documentsDocumentFolderValidationService.LintDocumentFoldersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDeliveries.SuggestDeliveriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDeliveries.SuggestDeliveriesRequest.Types.Filter
            {
                CreatedAfter = (reply1.DocumentFolder?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Venue?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (reply1.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _deliveriesDeliveryQueryService.SuggestDeliveriesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiIdentity.BrowseUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.BrowseUsersRequest.Types.Filter
            {
                CreatedAfter = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply1.DocumentFolder?.Location?.Region ?? ""),
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _identityUserQueryService.BrowseUsersAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.DeleteRegistrationReportResponse
        {
            EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "DeleteRegistrationReport", (request.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.FetchRegistrationReportResponse> FetchRegistrationReport(ApiCorporateevents.FetchRegistrationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling FetchRegistrationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "FetchRegistrationReport", "accepted", token);
        var input0 = new ApiMetrics.CheckMetricSeriesEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiMetrics.CheckMetricSeriesEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _metricsMetricSeriesValidationService.CheckMetricSeriesEligibilityAsync);
        var response = new ApiCorporateevents.FetchRegistrationReportResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.GenerateRegistrationReportResponse> GenerateRegistrationReport(ApiCorporateevents.GenerateRegistrationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateRegistrationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "GenerateRegistrationReport", "accepted", token);
        var input0 = new ApiDocuments.NormalizeDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _sendGenerateRegistrationReport0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ScheduleProblemReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ScheduleProblemReportRequest.Types.Details
            {
                Cron = (request.Details?.Cron ?? ""),
                Filters = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RecipientEmails = { (request.Details?.RecipientEmails ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        var named1 = _registry.Get<ApiItsm.ProblemReportingService.ProblemReportingServiceClient>("acme.itsm.v1.ProblemReportingService");
        using var rpc1 = named1.ScheduleProblemReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ScheduleEntitlementExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ScheduleEntitlementExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DocumentFolder?.Manager?.Email ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.DocumentFolder?.Manager?.UserId ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiLicensing.ScheduleEntitlementExpiryRequest, ApiLicensing.ScheduleEntitlementExpiryResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementLifecycleService", "ScheduleEntitlementExpiry",
            Wire.Marshaller<ApiLicensing.ScheduleEntitlementExpiryRequest>(), Wire.Marshaller<ApiLicensing.ScheduleEntitlementExpiryResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.GenerateRegistrationReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "GenerateRegistrationReport", (request.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists registrations a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListRegistrationReportsResponse> ListRegistrationReports(ApiCorporateevents.ListRegistrationReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListRegistrationReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "ListRegistrationReports", "accepted", token);
        var input0 = new ApiCorporateevents.CalculateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.CalculateCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiCorporateevents.CalculateCorporateEventRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _corporateeventsCorporateEventCalculationService.CalculateCorporateEventAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ListRegistrationReportsResponse
        {
            Registrations = { new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (reply0.CorporateEvent?.CorporateEventId ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ListRegistrationReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ScheduleRegistrationReportResponse> ScheduleRegistrationReport(ApiCorporateevents.ScheduleRegistrationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleRegistrationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "ScheduleRegistrationReport", "accepted", token);
        var input0 = new ApiItsm.ShareChangeRequestReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ShareChangeRequestReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _itsmChangeRequestReportingService.ShareChangeRequestReportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.PullVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.PullVenuesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ChangeRequest?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
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
        using var rpc1 = _corporateeventsVenueSyncService.PullVenuesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRisk.WatchRiskRegistersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply1.Venue?.Owner?.Email ?? ""),
                Name = (reply0.ChangeRequest?.Requester?.Name ?? new ApiCommon.PersonName()),
            },
        };
        using var stream2 = _riskRiskRegisterService.WatchRiskRegisters(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchRiskRegisters frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiHelpdesk.ResumeTicketWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (reply1.Venue?.Owner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                },
            },
        };
        var reply3 = await SendScheduleRegistrationReport3Async(input3, token);
        var response = new ApiCorporateevents.ScheduleRegistrationReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply3.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ScheduleRegistrationReport", (request.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ShareRegistrationReportResponse> ShareRegistrationReport(ApiCorporateevents.ShareRegistrationReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareRegistrationReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "ShareRegistrationReport", "accepted", token);
        var input0 = new ApiAuthorization.CountAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.CountAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _authorizationAccessPolicyQueryService.CountAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.ShareRegistrationReportResponse
        {
            Outcome = new ApiCorporateevents.ShareRegistrationReportResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ShareRegistrationReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of registrations as they happen.
    /// </summary>
    public override async Task StreamRegistrationMetrics(ApiCorporateevents.StreamRegistrationMetricsRequest request, IServerStreamWriter<ApiCorporateevents.StreamRegistrationMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamRegistrationMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", "batch",
            "StreamRegistrationMetrics", "accepted", token);
        var input0 = new ApiCorporateevents.NormalizeVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.NormalizeVenueRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.NormalizeVenueRequest>();
        using var rpc0 = _corporateeventsVenueValidationService.NormalizeVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.CancelDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CancelDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Venue?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyWorkflowService.CancelDevicePolicyWorkflowAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiVenues.SearchVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SearchVenuesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                OwnerId = (request.Filter?.OwnerId ?? ""),
            },
        };
        using var rpc2 = _venuesVenueService.SearchVenuesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiCorporateevents.StreamRegistrationMetricsResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Venue?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "StreamRegistrationMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiHelpdesk.ResumeTicketWorkflowResponse> SendScheduleRegistrationReport3Async(ApiHelpdesk.ResumeTicketWorkflowRequest request, CancellationToken token)
    {
        return await _registrationReportingEndpointSendScheduleRegistrationReport3Port.SendAsync(request, token);
    }
}
