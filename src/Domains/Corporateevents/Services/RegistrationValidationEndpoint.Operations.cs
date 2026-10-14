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
using ApiBoxoffice = Acme.Boxoffice.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Corporateevents;

public sealed partial class RegistrationValidationEndpoint
{
    /// <summary>
    /// Checks a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.CheckRegistrationEligibilityResponse> CheckRegistrationEligibility(ApiCorporateevents.CheckRegistrationEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckRegistrationEligibility for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CountUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.CountUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _identityUserQueryService.CountUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.RecalculateRegistrationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Bank = new ApiCorporateevents.RegistrationBankFunding
                {
                    Iban = (request.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (request.Registration?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationCalculationService.RecalculateRegistrationsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.CheckRegistrationEligibilityResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply1.Registration?.Holder?.DisplayName ?? ""),
                },
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "CheckRegistrationEligibility", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ExplainRegistrationDecisionResponse> ExplainRegistrationDecision(ApiCorporateevents.ExplainRegistrationDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainRegistrationDecision for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiCorporateevents.RecalculateRegistrationsResponse();
        foreach (var item in (request.Registration?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.RegistrationLimit>()))
        {
            var input0 = new ApiCorporateevents.RecalculateRegistrationsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Limits = { new ApiCorporateevents.RegistrationLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc0 = _corporateeventsRegistrationCalculationService.RecalculateRegistrationsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiBoxoffice.SearchBoxOfficeSalesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiBoxoffice.SearchBoxOfficeSalesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Registration?.Holder?.StartsAt ?? new Timestamp()),
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Registration?.Holder?.CorporateEventId ?? ""),
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _boxofficeBoxOfficeSaleService.SearchBoxOfficeSalesAsync);
        var input2 = new ApiLicensing.CompareLicenseAssignmentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Bank = new ApiLicensing.LicenseAssignmentBankSettlement
                {
                    AccountHolder = (request.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (request.Registration?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                    Bic = (reply0.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (reply0.Registration?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                    Iban = (request.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (request.Registration?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingLicenseAssignmentCalculationService.CompareLicenseAssignmentOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiCorporateevents.ExplainRegistrationDecisionResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Labels = { (request.Registration?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ExplainRegistrationDecision", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.GetRegistrationRulesResponse> GetRegistrationRules(ApiCorporateevents.GetRegistrationRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.RegistrationId, "registration_id");
        _logger.LogInformation("Handling GetRegistrationRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "registration", request.RegistrationId,
            "GetRegistrationRules", "accepted", token);
        var input0 = new ApiCorporateevents.EstimateRegistrationRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient>("acme.corporateevents.v1.RegistrationCalculationService");
        using var rpc0 = named0.EstimateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiCorporateevents.GetRegistrationRulesResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (reply0.Registration?.Holder?.DisplayName ?? ""),
                },
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
    /// Lints a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.LintRegistrationsResponse> LintRegistrations(ApiCorporateevents.LintRegistrationsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintRegistrations for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EstimateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Registration?.Audit?.Etag ?? ""),
                },
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    HolderName = (request.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Card => (request.Registration?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Registration?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsRegistrationCalculationService.EstimateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ScoreIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Incident = new ApiItsm.Incident
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Registration?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Registration?.Contact?.Email ?? ""),
                    PreferredChannel = (request.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiItsm.ScoreIncidentRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _itsmIncidentValidationService.ScoreIncidentAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.LintRegistrationsResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                CurrencyCode = (request.Registration?.CurrencyCode ?? ""),
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (reply0.Registration?.Holder?.CorporateEventId ?? ""),
                    DisplayName = (reply1.Incident?.Customer?.DisplayName ?? ""),
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
            "audit", "LintRegistrations", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.NormalizeRegistrationResponse> NormalizeRegistration(ApiCorporateevents.NormalizeRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EstimateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Bank = new ApiCorporateevents.RegistrationBankFunding
                {
                    AccountHolder = (request.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Bank => (request.Registration?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Registration?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    CorporateEventId = (request.Registration?.Holder?.CorporateEventId ?? ""),
                },
            },
        };
        using var rpc0 = _corporateeventsRegistrationCalculationService.EstimateRegistrationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CheckServiceAssetEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckServiceAssetEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Registration?.Contact?.Phone ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _itsmServiceAssetValidationService.CheckServiceAssetEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.NormalizeRegistrationToQuoteCorporateEvent2(reply0);
        using var rpc2 = _corporateeventsCorporateEventCalculationService.QuoteCorporateEventAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiCorporateevents.RecalculateRegistrationsResponse();
        foreach (var item in (request.Registration?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiCorporateevents.RegistrationLimit>()))
        {
            var input3 = new ApiCorporateevents.RecalculateRegistrationsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedAt = (request.Registration?.Audit?.UpdatedAt ?? new Timestamp()),
                    },
                    Limits = { new ApiCorporateevents.RegistrationLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                        Name = (item.Name ?? ""),
                        Period = (item.Period ?? new Duration()),
                    } },
                },
            };
            using var rpc3 = _corporateeventsRegistrationCalculationService.RecalculateRegistrationsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiCorporateevents.NormalizeRegistrationResponse
        {
            Outcome = new ApiCorporateevents.NormalizeRegistrationResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Available = (request.Registration?.Available ?? new ApiCommon.Money()),
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
                OpenedAt = (reply0.Registration?.OpenedAt ?? new Timestamp()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "NormalizeRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.PreviewRegistrationResponse> PreviewRegistration(ApiCorporateevents.PreviewRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CalculateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.CalculateKnowledgeArticleAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.CalculateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.KnowledgeArticle?.Counterparty?.Email ?? ""),
                    },
                } },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiCorporateevents.CalculateCorporateEventRequest>();
        using var rpc1 = _corporateeventsCorporateEventCalculationService.CalculateCorporateEventAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiCorporateevents.PreviewRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.KnowledgeArticle?.Audit?.Etag ?? ""),
                },
                Contact = (request.Registration?.Contact ?? new ApiCommon.ContactInfo()),
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "PreviewRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a registration.
    /// </summary>
    public override async Task<ApiCorporateevents.ValidateRegistrationResponse> ValidateRegistration(ApiCorporateevents.ValidateRegistrationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateRegistration for {Tenant}", requestContext.TenantId);
        if (request.Registration is { } record)
        {
            var snapshot = RegistrationMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiCorporateevents.EstimateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Registration?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Registration?.Holder?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiCorporateevents.EstimateVenueRequest>(json0);
        using var rpc0 = _corporateeventsVenueCalculationService.EstimateVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCorporateevents.EstimateVenueRequest>("acme.corporateevents.v1.RegistrationValidationService/ValidateRegistration/0", input0.Clone()), token);
        var response = new ApiCorporateevents.ValidateRegistrationResponse
        {
            Registration = new ApiCorporateevents.Registration
            {
                CurrencyCode = (request.Registration?.CurrencyCode ?? ""),
                Limits = { new ApiCorporateevents.RegistrationLimit
                {
                    Name = (reply0.Venue?.Name ?? ""),
                } },
                RegistrationId = (request.RegistrationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Registration?.RegistrationId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "registration",
            "audit", "ValidateRegistration", (request.Registration?.RegistrationId ?? ""), token);
        return response;
    }
}
