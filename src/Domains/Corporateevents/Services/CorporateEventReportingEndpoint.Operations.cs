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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiModelregistry = Acme.Modelregistry.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class CorporateEventReportingEndpoint
{
    /// <summary>
    /// Counts corporate events by status.
    /// </summary>
    public override async Task<ApiCorporateevents.CountCorporateEventTotalsResponse> CountCorporateEventTotals(ApiCorporateevents.CountCorporateEventTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountCorporateEventTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "CountCorporateEventTotals", "accepted", token);
        var input0 = new ApiCorporateevents.EstimateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _corporateeventsCorporateEventCalculationService.EstimateCorporateEventAsync);
        var response = new ApiCorporateevents.CountCorporateEventTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "CountCorporateEventTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.DeleteCorporateEventReportResponse> DeleteCorporateEventReport(ApiCorporateevents.DeleteCorporateEventReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling DeleteCorporateEventReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "DeleteCorporateEventReport", "accepted", token);
        var input0 = new ApiCorporateevents.FindRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindRegistrationsRequest.Types.Filter
            {
                OwnerId = (request.CorporateEventId ?? ""),
            },
        };
        using var rpc0 = _sendDeleteCorporateEventReport0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SendDocumentTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentNotificationService.SendDocumentTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.DeleteCorporateEventReportResponse
        {
            EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Etag ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "DeleteCorporateEventReport", (request.Etag ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.FetchCorporateEventReportResponse> FetchCorporateEventReport(ApiCorporateevents.FetchCorporateEventReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.CorporateEventId, "corporate_event_id");
        _logger.LogInformation("Handling FetchCorporateEventReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "FetchCorporateEventReport", "accepted", token);
        var input0 = new ApiCorporateevents.CheckVenueEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CheckVenueEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var descriptor0 = new Method<ApiCorporateevents.CheckVenueEligibilityRequest, ApiCorporateevents.CheckVenueEligibilityResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".VenueValidationService", "CheckVenueEligibility",
            Wire.Marshaller<ApiCorporateevents.CheckVenueEligibilityRequest>(), Wire.Marshaller<ApiCorporateevents.CheckVenueEligibilityResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ApproveEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ApproveEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (reply0.Venue?.Owner?.UserId ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.Venue?.Owner?.Email ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiLicensing.ApproveEntitlementRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _licensingEntitlementApprovalService.ApproveEntitlementAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.FetchCorporateEventReportResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (reply1.Entitlement?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.GenerateCorporateEventReportResponse> GenerateCorporateEventReport(ApiCorporateevents.GenerateCorporateEventReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateCorporateEventReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "GenerateCorporateEventReport", "accepted", token);
        var profile0 = new MappingProfile<ApiCorporateevents.GenerateCorporateEventReportRequest, ApiCorporateevents.SuggestRegistrationsRequest>(value => new ApiCorporateevents.SuggestRegistrationsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.SuggestRegistrationsRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                Labels = { (value.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _corporateeventsRegistrationQueryService.SuggestRegistrationsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CompareRegistrationOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (request.CorporateEventId ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationCalculationService.CompareRegistrationOptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.GetCorporateEventRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (request.CorporateEventId ?? ""),
        };
        using var rpc2 = _corporateeventsCorporateEventCalculationService.GetCorporateEventRatesAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiCorporateevents.GenerateCorporateEventReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "GenerateCorporateEventReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists corporate events a page at a time.
    /// </summary>
    public override async Task<ApiCorporateevents.ListCorporateEventReportsResponse> ListCorporateEventReports(ApiCorporateevents.ListCorporateEventReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListCorporateEventReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "ListCorporateEventReports", "accepted", token);
        var input0 = new ApiCorporateevents.RecalculateRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.RecalculateRegistrationsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await SendListCorporateEventReports0Async(input0, token);
        var response = new ApiCorporateevents.ListCorporateEventReportsResponse
        {
            CorporateEvents = { new ApiCorporateevents.CorporateEvent
            {
                CorporateEventId = (reply0.Registration?.Holder?.CorporateEventId ?? ""),
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = (reply0.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                } },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ListCorporateEventReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ScheduleCorporateEventReportResponse> ScheduleCorporateEventReport(ApiCorporateevents.ScheduleCorporateEventReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleCorporateEventReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "ScheduleCorporateEventReport", "accepted", token);
        var input0 = new ApiCorporateevents.PreviewCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _corporateeventsCorporateEventValidationService.PreviewCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CountRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.CountRegistrationsRequest.Types.Filter
            {
                CreatedAfter = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.CorporateEvent?.Organizer?.UserId ?? ""),
                Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.CountRegistrationsRequest>();
        using var rpc1 = _corporateeventsRegistrationQueryService.CountRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.PreviewUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.CorporateEvent?.Location?.Locality ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (reply0.CorporateEvent?.Organizer?.UserId ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiIdentity.PreviewUserRequest>(json2);
        using var rpc2 = _identityUserValidationService.PreviewUserAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiModelregistry.SimulateRegisteredModelRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Details = new ApiModelregistry.SimulateRegisteredModelRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Region = (reply2.User?.PrimaryAddress?.Region ?? ""),
            },
        };
        input3.RegisteredModel = new ApiModelregistry.RegisteredModel
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (reply2.User?.Audit?.CreatedAt ?? new Timestamp()),
                CreatedBy = (reply0.CorporateEvent?.Audit?.CreatedBy ?? ""),
            },
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
            },
        };
        using var rpc3 = _modelregistryRegisteredModelCalculationService.SimulateRegisteredModelAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.ScheduleCorporateEventReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ScheduleCorporateEventReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a corporate event.
    /// </summary>
    public override async Task<ApiCorporateevents.ShareCorporateEventReportResponse> ShareCorporateEventReport(ApiCorporateevents.ShareCorporateEventReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareCorporateEventReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", request.CorporateEventId,
            "ShareCorporateEventReport", "accepted", token);
        var input0 = new ApiIdentity.CountUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.CountUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Details?.Variables ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.TemplateId ?? ""),
            },
        };
        using var rpc0 = _identityUserQueryService.CountUsersAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.RetryLicenseAssignmentWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RetryLicenseAssignmentWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    Name = (request.Details?.Recipient?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentWorkflowService.RetryLicenseAssignmentWorkflowAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiCorporateevents.CompareVenueOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CompareVenueOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.LicenseAssignment?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (reply1.LicenseAssignment?.BillingAddress?.Recipient ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _corporateeventsVenueCalculationService.CompareVenueOptionsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiCorporateevents.ValidateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Bank = new ApiCorporateevents.RegistrationBankFunding
                {
                    AccountHolder = (reply1.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (reply1.LicenseAssignment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Recipient?.PreferredChannel ?? ""),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply1.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _corporateeventsRegistrationValidationService.ValidateRegistrationAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.ShareCorporateEventReportResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                CorporateEventId = (request.CorporateEventId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.Venue?.Address?.Recipient ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (reply3.Registration?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
            Outcome = new ApiCorporateevents.ShareCorporateEventReportResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "ShareCorporateEventReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of corporate events as they happen.
    /// </summary>
    public override async Task StreamCorporateEventMetrics(ApiCorporateevents.StreamCorporateEventMetricsRequest request, IServerStreamWriter<ApiCorporateevents.StreamCorporateEventMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamCorporateEventMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event", "batch",
            "StreamCorporateEventMetrics", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentsRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var rpc0 = _sendStreamCorporateEventMetrics0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.SummarizeRegistrationsRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.SummarizeRegistrationsRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var named1 = _registry.Get<ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient>("acme.corporateevents.v1.RegistrationQueryService");
        using var rpc1 = named1.SummarizeRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ApproveChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor2 = new Method<ApiItsm.ApproveChangeRequestRequest, ApiItsm.ApproveChangeRequestResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ChangeRequestApprovalService", "ApproveChangeRequest",
            Wire.Marshaller<ApiItsm.ApproveChangeRequestRequest>(), Wire.Marshaller<ApiItsm.ApproveChangeRequestResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.StreamCorporateEventMetricsResponse
        {
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (reply2.ChangeRequest?.Requester?.Phone ?? ""),
                    },
                } },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "corporate_event",
            "audit", "StreamCorporateEventMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiCorporateevents.RecalculateRegistrationsResponse> SendListCorporateEventReports0Async(ApiCorporateevents.RecalculateRegistrationsRequest request, CancellationToken token)
    {
        return await _corporateEventReportingEndpointSendListCorporateEventReports0Port.SendAsync(request, token);
    }
}
