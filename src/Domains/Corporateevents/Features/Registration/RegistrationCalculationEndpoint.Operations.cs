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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiPatients = Acme.Patients.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class RegistrationCalculationEndpoint
{
    /// <summary>
    /// Calculates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.CalculateRegistrationResponse> CalculateRegistration(ApiCorporateevents.CalculateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.StreamDocumentTemplateNoticesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentTemplateNoticesRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Registration?.Contact?.Email ?? ""),
            },
        };
        using var stream0 = _documentsDocumentTemplateNotificationService.StreamDocumentTemplateNotices(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentTemplateNotices frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiHelpdesk.CreateTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Registration?.Holder?.DisplayName ?? ""),
                    Email = (request.Registration?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.Registration?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiHelpdesk.CreateTicketRequest>();
        using var rpc1 = _helpdeskTicketService.CreateTicketAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CalculateRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (reply1.Ticket?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
                RegistrationId = (request.RegistrationId ?? ""),
                Status = (request.Registration?.Status ?? (ApiCorporateevents.RegistrationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "CalculateRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.CompareRegistrationOptionsResponse> CompareRegistrationOptions(ApiCorporateevents.CompareRegistrationOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareRegistrationOptions for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.RecalculateCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Registration?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiCorporateevents.RecalculateCorporateEventsRequest>(json0);
        using var rpc0 = _corporateeventsCorporateEventCalculationService.RecalculateCorporateEventsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTelematics.ExplainTelemetryReadingCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            TelemetryReading = new ApiTelematics.TelemetryReading
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiTelematics.GeoFenceRef
                {
                    DisplayName = (reply0.CorporateEvent?.Organizer?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _telematicsTelemetryReadingCalculationService.ExplainTelemetryReadingCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CompareRegistrationOptionsResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.CorporateEvent?.Audit?.CreatedBy ?? ""),
                },
                CurrencyCode = (request.Registration?.CurrencyCode ?? ""),
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply1.TelemetryReading?.Subject?.DisplayName ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "CompareRegistrationOptions", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.EstimateRegistrationResponse> EstimateRegistration(ApiCorporateevents.EstimateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.SimulateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (request.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                    },
                } },
            },
        };
        using var rpc0 = _corporateeventsCorporateEventCalculationService.SimulateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.QuoteCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Daily = new ApiCorporateevents.CorporateEventDailyRule
                {
                    AtHour = (reply0.CorporateEvent ?? new ApiCorporateevents.CorporateEvent()).RecurrenceCase switch
                    {
                        ApiCorporateevents.CorporateEvent.RecurrenceOneofCase.Daily => (reply0.CorporateEvent?.Daily?.AtHour ?? 0),
                        _ => 0
                    },
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Phone = (request.Registration?.Contact?.Phone ?? ""),
                        PreferredChannel = (request.Registration?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.QuoteCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventCalculationService.QuoteCorporateEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.EstimateRegistrationResponse
        {
            Breakdown = new ApiCorporateevents.EstimateRegistrationResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Contact = (request.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (reply1.CorporateEvent?.CorporateEventId ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "EstimateRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ExplainRegistrationCalculationResponse> ExplainRegistrationCalculation(ApiCorporateevents.ExplainRegistrationCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainRegistrationCalculation for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPatients.ClonePatientRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Patient = new ApiPatients.Patient
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Registration?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _patientsPatientService.ClonePatientAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiCorporateevents.RecalculateCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Patient?.PrimaryAddress?.CountryCode ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = (reply0.Patient?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Details?.Contact?.Phone ?? ""),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _corporateeventsCorporateEventCalculationService.RecalculateCorporateEventsAsync);
        var input2 = new ApiCorporateevents.QuoteCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Patient?.Audit?.CreatedBy ?? ""),
                },
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Patient?.RelationshipManager?.Email ?? ""),
                        Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                        Phone = (request.Registration?.Contact?.Phone ?? ""),
                    },
                } },
            },
        };
        using var rpc2 = _sendExplainRegistrationCalculation2(input2, new CallOptions(cancellationToken: token));
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ExplainRegistrationCalculationResponse
        {
            Outcome = new ApiCorporateevents.ExplainRegistrationCalculationResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.CorporateEvent?.Audit?.CreatedBy ?? ""),
                },
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ExplainRegistrationCalculation", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.GetRegistrationRatesResponse> GetRegistrationRates(ApiCorporateevents.GetRegistrationRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling GetRegistrationRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "GetRegistrationRates", "accepted", token);
        var input0 = new ApiCorporateevents.CalculateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
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
                    ParticipantId = (request.Context?.TraceId ?? ""),
                } },
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient>("acme.corporateevents.v1.CorporateEventCalculationService");
        using var rpc0 = named0.CalculateCorporateEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SyncDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SyncDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.CorporateEvent?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateSyncService.SyncDocumentTemplatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.GetRegistrationRatesResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.RecalculateRegistrationsResponse> RecalculateRegistrations(ApiCorporateevents.RecalculateRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateRegistrations for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ReconcileDocumentTemplatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ReconcileDocumentTemplatesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Registration?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentTemplateSyncService.ReconcileDocumentTemplatesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiCorporateevents.RecalculateRegistrationsRequest, ApiCorporateevents.GetCorporateEventRatesRequest>(value => new ApiCorporateevents.GetCorporateEventRatesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            CorporateEventId = (value.Registration?.Holder?.CorporateEventId ?? ""),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _corporateeventsCorporateEventCalculationService.GetCorporateEventRatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.RecalculateRegistrationsResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.CorporateEvent?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.DocumentTemplate?.Audit?.UpdatedBy ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
                Status = (request.Registration?.Status ?? (ApiCorporateevents.RegistrationStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "RecalculateRegistrations", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
}
