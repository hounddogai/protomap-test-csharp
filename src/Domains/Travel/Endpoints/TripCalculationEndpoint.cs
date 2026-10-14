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
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTravel = Acme.Travel.V1;
using ApiYards = Acme.Yards.V1;

namespace Acme.Workforce.Travel;

/// <summary>
/// Computes the figures of a trip, such as totals, taxes, and estimates.
/// </summary>
public sealed class TripCalculationEndpoint : ApiTravel.TripCalculationService.TripCalculationServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly NotificationBus _bus;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient _corporateeventsRegistrationValidationService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient _documentsDocumentTemplateCalculationService;
    private readonly ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient _documentsDocumentTemplateNotificationService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient _helpdeskSlaPolicyCalculationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ILogger<TripCalculationEndpoint> _logger;
    private readonly ApiNotifications.NotificationValidationService.NotificationValidationServiceClient _notificationsNotificationValidationService;
    private readonly ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient _pipelinesPipelineCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ITripCalculationEndpointSendExplainTripCalculation3Port _tripCalculationEndpointSendExplainTripCalculation3Port;
    private readonly ApiYards.YardQueryService.YardQueryServiceClient _yardsYardQueryService;

    public TripCalculationEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        NotificationBus bus,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.RegistrationValidationService.RegistrationValidationServiceClient corporateeventsRegistrationValidationService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentTemplateCalculationService.DocumentTemplateCalculationServiceClient documentsDocumentTemplateCalculationService,
        ApiDocuments.DocumentTemplateNotificationService.DocumentTemplateNotificationServiceClient documentsDocumentTemplateNotificationService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient helpdeskSlaPolicyCalculationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ILogger<TripCalculationEndpoint> logger,
        ApiNotifications.NotificationValidationService.NotificationValidationServiceClient notificationsNotificationValidationService,
        ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient pipelinesPipelineCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ITripCalculationEndpointSendExplainTripCalculation3Port tripCalculationEndpointSendExplainTripCalculation3PortAdapter,
        ApiYards.YardQueryService.YardQueryServiceClient yardsYardQueryService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _bus = bus;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsRegistrationValidationService = corporateeventsRegistrationValidationService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentTemplateCalculationService = documentsDocumentTemplateCalculationService;
        _documentsDocumentTemplateNotificationService = documentsDocumentTemplateNotificationService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _helpdeskSlaPolicyCalculationService = helpdeskSlaPolicyCalculationService;
        _identityUserService = identityUserService;
        _invoker = invoker;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _logger = logger;
        _notificationsNotificationValidationService = notificationsNotificationValidationService;
        _pipelinesPipelineCalculationService = pipelinesPipelineCalculationService;
        _repository = repository;
        _settings = settings;
        _tripCalculationEndpointSendExplainTripCalculation3Port = tripCalculationEndpointSendExplainTripCalculation3PortAdapter;
        _yardsYardQueryService = yardsYardQueryService;
    }
    /// <summary>
    /// Calculates a trip.
    /// </summary>
    public override async Task<ApiTravel.CalculateTripResponse> CalculateTrip(ApiTravel.CalculateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ScheduleDocumentExportRequest();
        input0.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiDocuments.ScheduleDocumentExportRequest.Types.Details
        {
            Filters = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        input0.Reason = (request.Reason ?? "");
        using var rpc0 = _documentsDocumentExportService.ScheduleDocumentExportAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()).Select(async item =>
        {
            var input1 = new ApiCorporateevents.ValidateRegistrationRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Registration = new ApiCorporateevents.Registration
                {
                    Card = new ApiCorporateevents.RegistrationCardFunding
                    {
                        Token = (request.Trip?.Card?.Token ?? ""),
                    },
                    Limits = { new ApiCorporateevents.RegistrationLimit
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc1 = _corporateeventsRegistrationValidationService.ValidateRegistrationAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiCorporateevents.ValidateRegistrationResponse();
        var input2 = new ApiDocuments.CompareDocumentTemplateOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentTemplateOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Trip?.BillingAddress?.CountryCode ?? ""),
                },
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Registration?.Audit?.CreatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (request.Trip?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentTemplateCalculationService.CompareDocumentTemplateOptionsAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.CalculateTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply2.DocumentTemplate?.BillingAddress?.Region ?? ""),
                },
                Number = (request.Trip?.Number ?? ""),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CalculateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a trip.
    /// </summary>
    public override async Task<ApiTravel.CompareTripOptionsResponse> CompareTripOptions(ApiTravel.CompareTripOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareTripOptions for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiYards.BrowseYardsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiYards.BrowseYardsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.IssuedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Trip?.Counterparty?.EmployeeId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _yardsYardQueryService.BrowseYardsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiNotifications.PreviewNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _notificationsNotificationValidationService.PreviewNotificationAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.Notification?.Assignee?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CompareComplianceScanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var reply3 = new ApiLedger.SummarizeLedgerAccountsResponse();
        foreach (var item in (request.Trip?.Lines ?? new Google.Protobuf.Collections.RepeatedField<ApiTravel.TripLine>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input3 = new ApiLedger.SummarizeLedgerAccountsRequest
            {
                Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLedger.SummarizeLedgerAccountsRequest.Types.Details
                {
                    Filters = { (item.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                },
                Reason = (request.Reason ?? ""),
            };
            using var rpc3 = _ledgerLedgerAccountQueryService.SummarizeLedgerAccountsAsync(input3, cancellationToken: token);
            reply3 = await rpc3.ResponseAsync;
        }
        var response = new ApiTravel.CompareTripOptionsResponse
        {
            Breakdown = new ApiTravel.CompareTripOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                Total = (reply2.Breakdown?.Total ?? new ApiCommon.Money()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "CompareTripOptions", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a trip.
    /// </summary>
    public override async Task<ApiTravel.EstimateTripResponse> EstimateTrip(ApiTravel.EstimateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ExplainEntitlementDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Trip?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiLicensing.ExplainEntitlementDecisionRequest, ApiLicensing.ExplainEntitlementDecisionResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementValidationService", "ExplainEntitlementDecision",
            Wire.Marshaller<ApiLicensing.ExplainEntitlementDecisionRequest>(), Wire.Marshaller<ApiLicensing.ExplainEntitlementDecisionResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPipelines.CalculatePipelineRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPipelines.CalculatePipelineRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Trip?.BillingAddress?.Region ?? ""),
                },
            },
            Pipeline = new ApiPipelines.Pipeline
            {
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiPipelines.CalculatePipelineRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _pipelinesPipelineCalculationService.CalculatePipelineAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiTravel.EstimateTripResponse
        {
            Breakdown = new ApiTravel.EstimateTripResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Pipeline?.Audit?.CreatedBy ?? ""),
                },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "EstimateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a trip.
    /// </summary>
    public override async Task<ApiTravel.ExplainTripCalculationResponse> ExplainTripCalculation(ApiTravel.ExplainTripCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainTripCalculation for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiTravel.ExplainTripCalculationRequest, ApiYards.CountYardsRequest>(value => new ApiYards.CountYardsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiYards.CountYardsRequest.Types.Filter
            {
                CreatedAfter = (value.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (value.Trip?.BillingAddress?.Region ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _yardsYardQueryService.CountYardsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateSubscriptionsRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc1 = _documentsDocumentTemplateNotificationService.ListDocumentTemplateSubscriptionsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiDocuments.SimulateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SimulateSignatureRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Trip?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Trip?.BillingAddress?.Region ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc2 = _documentsSignatureCalculationService.SimulateSignatureAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiCorporateevents.LintCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Contact?.Email ?? ""),
                        PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                    },
                } },
            },
        };
        var reply3 = await SendExplainTripCalculation3Async(input3, token);
        var response = new ApiTravel.ExplainTripCalculationResponse
        {
            Outcome = new ApiTravel.ExplainTripCalculationResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply3.CorporateEvent?.Location?.CountryCode ?? ""),
                },
                Metadata = { (request.Trip?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "ExplainTripCalculation", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a trip.
    /// </summary>
    public override async Task<ApiTravel.GetTripRatesResponse> GetTripRates(ApiTravel.GetTripRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.TripId, "trip_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetTripRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "trip", request.TripId,
            "GetTripRates", "accepted", token);
        var input0 = new ApiHelpdesk.EstimateSlaPolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Parameters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _helpdeskSlaPolicyCalculationService.EstimateSlaPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiTravel.GetTripRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Lines = { new ApiTravel.TripLine
                {
                    Description = (reply0.SlaPolicy?.Description ?? ""),
                } },
                TripId = (request.TripId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a trip.
    /// </summary>
    public override async Task<ApiTravel.QuoteTripResponse> QuoteTrip(ApiTravel.QuoteTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.BatchGetUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiIdentity.BatchGetUsersRequest>();
        using var rpc0 = _identityUserService.BatchGetUsersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiIdentity.BatchGetUsersRequest>("acme.travel.v1.TripCalculationService/QuoteTrip/0", input0.Clone()), token);
        var input1 = new ApiCorporateevents.ValidateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Bank = new ApiCorporateevents.RegistrationBankFunding
                {
                    Bic = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Bank => (request.Trip?.Bank?.Bic ?? ""),
                        _ => ""
                    },
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _corporateeventsRegistrationValidationService.ValidateRegistrationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.NormalizeRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.NormalizeRegistrationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Trip?.Counterparty?.Email ?? ""),
                    PreferredChannel = (reply1.Registration?.Contact?.PreferredChannel ?? ""),
                },
            },
            Registration = new ApiCorporateevents.Registration
            {
                Card = new ApiCorporateevents.RegistrationCardFunding
                {
                    Last4 = (reply1.Registration ?? new ApiCorporateevents.Registration()).PaymentCase switch
                    {
                        ApiCorporateevents.Registration.FundingOneofCase.Card => (reply1.Registration?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                },
                Holder = new ApiCorporateevents.CorporateEventRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _corporateeventsRegistrationValidationService.NormalizeRegistrationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiAudit.LintAuditEventsRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiAudit.AuditTrailRef
                {
                    DisplayName = (reply1.Registration?.Holder?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAudit.LintAuditEventsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Registration?.Contact?.Email ?? ""),
                    Phone = (reply1.Registration?.Contact?.Phone ?? ""),
                },
            },
        };
        using var rpc3 = _auditAuditEventValidationService.LintAuditEventsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiTravel.QuoteTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply2.Registration?.Holder?.DisplayName ?? ""),
                    Email = (reply1.Registration?.Contact?.Email ?? ""),
                },
                Lines = { new ApiTravel.TripLine
                {
                    Attributes = { (reply3.AuditEvent?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
                Status = (request.Trip?.Status ?? (ApiTravel.TripStatus)0),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "QuoteTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a trip.
    /// </summary>
    public override async Task<ApiTravel.RecalculateTripsResponse> RecalculateTrips(ApiTravel.RecalculateTripsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateTrips for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.ListUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiIdentity.ListUsersRequest.Types.Filter
            {
                CreatedAfter = (request.Trip?.DueAt ?? new Timestamp()),
                Region = (request.Trip?.BillingAddress?.Region ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _identityUserService.ListUsersAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiTravel.RecalculateTripsResponse
        {
            Breakdown = new ApiTravel.RecalculateTripsResponse.Types.Breakdown
            {
                Total = (request.Trip?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Trip = new ApiTravel.Trip
            {
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "RecalculateTrips", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a trip.
    /// </summary>
    public override async Task<ApiTravel.SimulateTripResponse> SimulateTrip(ApiTravel.SimulateTripRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateTrip for {Tenant}", requestContext.TenantId);
        if (request.Trip is { } record)
        {
            var snapshot = TripMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.NormalizeDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                Bank = new ApiDocuments.DocumentBankSettlement
                {
                    Iban = (request.Trip ?? new ApiTravel.Trip()).PaymentCase switch
                    {
                        ApiTravel.Trip.SettlementOneofCase.Bank => (request.Trip?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (request.Trip?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentValidationService.NormalizeDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.QuoteDocumentTemplateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Trip?.BillingAddress?.Locality ?? ""),
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    Path = (reply0.Document?.Counterparty?.Path ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _documentsDocumentTemplateCalculationService.QuoteDocumentTemplateAsync);
        var response = new ApiTravel.SimulateTripResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            Trip = new ApiTravel.Trip
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Document?.Audit?.Etag ?? ""),
                },
                IssuedAt = (request.Trip?.IssuedAt ?? new Timestamp()),
                TripId = (request.TripId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Trip?.VoucherCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "trip",
            "audit", "SimulateTrip", (request.Trip?.VoucherCode ?? ""), token);
        return response;
    }
    private async Task<ApiCorporateevents.LintCorporateEventsResponse> SendExplainTripCalculation3Async(ApiCorporateevents.LintCorporateEventsRequest request, CancellationToken token)
    {
        return await _tripCalculationEndpointSendExplainTripCalculation3Port.SendAsync(request, token);
    }
}
