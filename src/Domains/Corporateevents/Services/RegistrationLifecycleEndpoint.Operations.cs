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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class RegistrationLifecycleEndpoint
{
    /// <summary>
    /// Activates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ActivateRegistrationResponse> ActivateRegistration(ApiCorporateevents.ActivateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling ActivateRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "ActivateRegistration", "accepted", token);
        var input0 = new ApiCorporateevents.StreamVenueMetricsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.NotifyContact?.Phone ?? ""),
            },
        };
        using var stream0 = _corporateeventsVenueReportingService.StreamVenueMetrics(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamVenueMetrics frame {Type}", change0.GetType().Name);
        }
        var response = new ApiCorporateevents.ActivateRegistrationResponse
        {
            Outcome = new ApiCorporateevents.ActivateRegistrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ActivateRegistration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ExpireRegistrationResponse> ExpireRegistration(ApiCorporateevents.ExpireRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling ExpireRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "ExpireRegistration", "accepted", token);
        var input0 = request.ExpireRegistrationToSummarizeCorporateEvents0();
        using var rpc0 = _corporateeventsCorporateEventQueryService.SummarizeCorporateEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ShareVenueReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareVenueReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsVenueReportingService.ShareVenueReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.PushVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.PushVenueRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Venue ?? new ApiCorporateevents.Venue()).LocatorCase switch
                    {
                        ApiCorporateevents.Venue.LocatorOneofCase.Address => (reply1.Venue?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _corporateeventsVenueSyncService.PushVenueAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ExpireRegistrationResponse
        {
            Outcome = new ApiCorporateevents.ExpireRegistrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Venue?.Audit?.UpdatedBy ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ExpireRegistration", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.GetRegistrationLifecycleResponse> GetRegistrationLifecycle(ApiCorporateevents.GetRegistrationLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling GetRegistrationLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "GetRegistrationLifecycle", "accepted", token);
        var input0 = new ApiCorporateevents.ListCorporateEventReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.ListCorporateEventReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.RegistrationId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiCorporateevents.ListCorporateEventReportsRequest>();
        using var rpc0 = _corporateeventsCorporateEventReportingService.ListCorporateEventReportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.GetRegistrationLifecycleResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ReinstateRegistrationResponse> ReinstateRegistration(ApiCorporateevents.ReinstateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling ReinstateRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "ReinstateRegistration", "accepted", token);
        var input0 = new ApiCorporateevents.EstimateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiCorporateevents.EstimateCorporateEventRequest>(json0);
        using var rpc0 = _corporateeventsCorporateEventCalculationService.EstimateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.BrowseRegistrationsRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Filter = new ApiCorporateevents.BrowseRegistrationsRequest.Types.Filter
        {
            CreatedAfter = (reply0.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
            OwnerId = (reply0.Result?.CorrelationId ?? ""),
            Region = (reply0.CorporateEvent?.Location?.Region ?? ""),
        };
        using var rpc1 = _corporateeventsRegistrationQueryService.BrowseRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.NormalizeCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NewOwner?.Email ?? ""),
                        PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        using var rpc2 = _corporateeventsCorporateEventValidationService.NormalizeCorporateEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ReinstateRegistrationResponse
        {
            Outcome = new ApiCorporateevents.ReinstateRegistrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.CorporateEvent?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.CorporateEvent?.Organizer?.Email ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ReinstateRegistration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.RenewRegistrationResponse> RenewRegistration(ApiCorporateevents.RenewRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling RenewRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "RenewRegistration", "accepted", token);
        var input0 = new ApiCorporateevents.FindCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCorporateevents.FindCorporateEventsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _corporateeventsCorporateEventQueryService.FindCorporateEventsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiCorporateevents.RenewRegistrationResponse
        {
            Outcome = new ApiCorporateevents.RenewRegistrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "RenewRegistration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ScheduleRegistrationExpiryResponse> ScheduleRegistrationExpiry(ApiCorporateevents.ScheduleRegistrationExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleRegistrationExpiry for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.CreateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Details?.Notify?.Phone ?? ""),
                    },
                } },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventService.CreateCorporateEventAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ShareRegistrationReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ShareRegistrationReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Name = (request.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Registration?.Contact?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationReportingService.ShareRegistrationReportAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiCorporateevents.ScheduleRegistrationExpiryResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Registration?.Audit?.Etag ?? ""),
                },
                Balance = (request.Registration?.Balance ?? new ApiCommon.Money()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ScheduleRegistrationExpiry", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.SuspendRegistrationResponse> SuspendRegistration(ApiCorporateevents.SuspendRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling SuspendRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "SuspendRegistration", "accepted", token);
        var input0 = new ApiPatients.FindPatientsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPatients.FindPatientsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc0 = _sendSuspendRegistration0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.SuspendRegistrationResponse
        {
            Outcome = new ApiCorporateevents.SuspendRegistrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "SuspendRegistration", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.TerminateRegistrationResponse> TerminateRegistration(ApiCorporateevents.TerminateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling TerminateRegistration for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "TerminateRegistration", "accepted", token);
        var input0 = new ApiCorporateevents.ExplainCorporateEventCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.NewOwner?.Email ?? ""),
                    },
                } },
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient>("acme.corporateevents.v1.CorporateEventCalculationService");
        using var rpc0 = named0.ExplainCorporateEventCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ScheduleVenueReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.ScheduleVenueReportRequest.Types.Details
            {
                Period = (reply0.CorporateEvent?.Window ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var descriptor1 = new Method<ApiCorporateevents.ScheduleVenueReportRequest, ApiCorporateevents.ScheduleVenueReportResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".VenueReportingService", "ScheduleVenueReport",
            Wire.Marshaller<ApiCorporateevents.ScheduleVenueReportRequest>(), Wire.Marshaller<ApiCorporateevents.ScheduleVenueReportResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ReconcileProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ReconcileProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc2 = _itsmProblemSyncService.ReconcileProblemsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiCorporateevents.TerminateRegistrationRequest, ApiCorporateevents.DeleteRegistrationReportRequest>(value => new ApiCorporateevents.DeleteRegistrationReportRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Etag = (reply2.Problem?.Audit?.Etag ?? ""),
            Reason = (value.Reason ?? ""),
            RegistrationId = (value.RegistrationId ?? ""),
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _corporateeventsRegistrationReportingService.DeleteRegistrationReportAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiCorporateevents.TerminateRegistrationResponse
        {
            Outcome = new ApiCorporateevents.TerminateRegistrationResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (reply0.CorporateEvent?.CorporateEventId ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "TerminateRegistration", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
}
