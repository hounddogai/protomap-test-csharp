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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelehealth = Acme.Telehealth.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Computes the figures of a service asset, such as totals, taxes, and estimates.
/// </summary>
public sealed class ServiceAssetCalculationEndpoint : ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceBase
{
    private readonly ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient _authorizationAccessPolicyQueryService;
    private readonly NotificationBus _bus;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient _devicesComplianceScanValidationService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient _licensingSoftwareLicenseValidationService;
    private readonly ILogger<ServiceAssetCalculationEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiVenues.CreateVenueRequest, CallOptions, AsyncUnaryCall<ApiVenues.CreateVenueResponse>> _sendQuoteServiceAsset0;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient _telehealthVideoVisitCalculationService;
    private readonly ApiVenues.VenueService.VenueServiceClient _venuesVenueService;

    public ServiceAssetCalculationEndpoint(
        ApiAuthorization.AccessPolicyQueryService.AccessPolicyQueryServiceClient authorizationAccessPolicyQueryService,
        NotificationBus bus,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ComplianceScanValidationService.ComplianceScanValidationServiceClient devicesComplianceScanValidationService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseValidationService.SoftwareLicenseValidationServiceClient licensingSoftwareLicenseValidationService,
        ILogger<ServiceAssetCalculationEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiTelehealth.VideoVisitCalculationService.VideoVisitCalculationServiceClient telehealthVideoVisitCalculationService,
        ApiVenues.VenueService.VenueServiceClient venuesVenueService)
    {
        _authorizationAccessPolicyQueryService = authorizationAccessPolicyQueryService;
        _bus = bus;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesComplianceScanValidationService = devicesComplianceScanValidationService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _invoker = invoker;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseValidationService = licensingSoftwareLicenseValidationService;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _telehealthVideoVisitCalculationService = telehealthVideoVisitCalculationService;
        _venuesVenueService = venuesVenueService;
        _sendQuoteServiceAsset0 = _venuesVenueService.CreateVenueAsync;
    }
    /// <summary>
    /// Calculates a service asset.
    /// </summary>
    public override async Task<ApiItsm.CalculateServiceAssetResponse> CalculateServiceAsset(ApiItsm.CalculateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CalculateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (request.ServiceAsset?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.ServiceAsset?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _licensingSoftwareLicenseCalculationService.CalculateSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.CalculateServiceAssetToEstimateChangeRequest1(reply0);
        using var rpc1 = _itsmChangeRequestCalculationService.EstimateChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.SimulateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.ChangeRequest?.Requester?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SimulateChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _itsmChangeRequestCalculationService.SimulateChangeRequestAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.CalculateServiceAssetResponse
        {
            Breakdown = new ApiItsm.CalculateServiceAssetResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ChangeRequest?.Assignee?.DisplayName ?? ""),
                    Email = (reply1.ChangeRequest?.Requester?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Status = (request.ServiceAsset?.Status ?? (ApiItsm.ServiceAssetStatus)0),
                Tags = { (reply2.ChangeRequest?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "CalculateServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a service asset.
    /// </summary>
    public override async Task<ApiItsm.CompareServiceAssetOptionsResponse> CompareServiceAssetOptions(ApiItsm.CompareServiceAssetOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareServiceAssetOptions for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CalculateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (request.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _itsmChangeRequestCalculationService.CalculateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTelehealth.EstimateVideoVisitRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTelehealth.EstimateVideoVisitRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                    Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _telehealthVideoVisitCalculationService.EstimateVideoVisitAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CompareServiceAssetOptionsResponse
        {
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.VideoVisit?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = (request.ServiceAsset?.Owner ?? new ApiIdentity.UserRef()),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Tags = { (reply0.ChangeRequest?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "CompareServiceAssetOptions", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a service asset.
    /// </summary>
    public override async Task<ApiItsm.EstimateServiceAssetResponse> EstimateServiceAsset(ApiItsm.EstimateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CheckSoftwareLicenseEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ServiceAsset?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLicensing.CheckSoftwareLicenseEligibilityRequest>(json0);
        using var rpc0 = _licensingSoftwareLicenseValidationService.CheckSoftwareLicenseEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.CompareChangeRequestOptionsRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                Customer = new ApiItsm.IncidentRef
                {
                    DisplayName = (request.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (reply0.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CompareChangeRequestOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
        };
        using var rpc1 = _itsmChangeRequestCalculationService.CompareChangeRequestOptionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CalculateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseCalculationService.CalculateSoftwareLicenseAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.RecordDocumentTemplateVersionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.RecordDocumentTemplateVersionRequest.Types.Details
            {
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Bank = new ApiDocuments.DocumentTemplateBankSettlement
                {
                    AccountHolder = (reply2.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Bank => (reply2.SoftwareLicense?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiDocuments.DocumentFolderRef
                {
                    DisplayName = (reply1.ChangeRequest?.Assignee?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _documentsDocumentTemplateHistoryService.RecordDocumentTemplateVersionAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.EstimateServiceAssetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply3.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.ChangeRequest?.Audit?.UpdatedBy ?? ""),
                },
                Name = (request.ServiceAsset?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ChangeRequest?.Customer?.DisplayName ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "EstimateServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a service asset.
    /// </summary>
    public override async Task<ApiItsm.ExplainServiceAssetCalculationResponse> ExplainServiceAssetCalculation(ApiItsm.ExplainServiceAssetCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainServiceAssetCalculation for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    Unit = (request.ServiceAsset?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CompareComplianceScanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ExplainServiceAssetCalculationResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Specs = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ExplainServiceAssetCalculation", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a service asset.
    /// </summary>
    public override async Task<ApiItsm.GetServiceAssetRatesResponse> GetServiceAssetRates(ApiItsm.GetServiceAssetRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetServiceAssetRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "GetServiceAssetRates", "accepted", token);
        var input0 = new ApiVenues.SearchVenuesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiVenues.SearchVenuesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.ServiceAssetId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _venuesVenueService.SearchVenuesAsync);
        var response = new ApiItsm.GetServiceAssetRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a service asset.
    /// </summary>
    public override async Task<ApiItsm.QuoteServiceAssetResponse> QuoteServiceAsset(ApiItsm.QuoteServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiVenues.CreateVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.ServiceAsset?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendQuoteServiceAsset0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.ResolveLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named1 = _registry.Get<ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient>("acme.ledger.v1.LedgerAccountQueryService");
        using var rpc1 = named1.ResolveLedgerAccountsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.CompareChangeRequestOptionsRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply0.Venue?.Owner?.Email ?? ""),
                },
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (reply0.Venue?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CompareChangeRequestOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiItsm.CompareChangeRequestOptionsRequest, ApiItsm.CompareChangeRequestOptionsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ChangeRequestCalculationService", "CompareChangeRequestOptions",
            Wire.Marshaller<ApiItsm.CompareChangeRequestOptionsRequest>(), Wire.Marshaller<ApiItsm.CompareChangeRequestOptionsResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.QuoteServiceAssetResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply2.ChangeRequest?.Audit?.CreatedBy ?? ""),
                },
                Capacity = (request.ServiceAsset?.Capacity ?? 0L),
                Dimensions = new ApiItsm.ServiceAssetDimensions
                {
                    Height = (reply0.Venue?.Dimensions?.Height ?? 0d),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "QuoteServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a service asset.
    /// </summary>
    public override async Task<ApiItsm.RecalculateServiceAssetsResponse> RecalculateServiceAssets(ApiItsm.RecalculateServiceAssetsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateServiceAssets for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiTelehealth.ExplainVideoVisitCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VideoVisit = new ApiTelehealth.VideoVisit
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiTelehealth.ExplainVideoVisitCalculationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _telehealthVideoVisitCalculationService.ExplainVideoVisitCalculationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.RecalculateServiceAssetsRequest, ApiItsm.SimulateChangeRequestRequest>(value => new ApiItsm.SimulateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiItsm.ChangeRequestComment
                {
                    CreatedAt = (reply0.VideoVisit?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SimulateChangeRequestRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (value.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmChangeRequestCalculationService.SimulateChangeRequestAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.ExplainComplianceScanDecisionRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainComplianceScanDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanValidationService.ExplainComplianceScanDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.RecalculateServiceAssetsToSummarizeAccessPolicies3();
        using var rpc3 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiAuthorization.SummarizeAccessPoliciesRequest>("acme.itsm.v1.ServiceAssetCalculationService/RecalculateServiceAssets/3", input3.Clone()), token);
        var response = new ApiItsm.RecalculateServiceAssetsResponse
        {
            Breakdown = new ApiItsm.RecalculateServiceAssetsResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ChangeRequest?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.VideoVisit?.Organizer?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "RecalculateServiceAssets", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a service asset.
    /// </summary>
    public override async Task<ApiItsm.SimulateServiceAssetResponse> SimulateServiceAsset(ApiItsm.SimulateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateServiceAsset for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateComplianceScansRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SimulateServiceAssetResponse
        {
            Breakdown = new ApiItsm.SimulateServiceAssetResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
                Status = (request.ServiceAsset?.Status ?? (ApiItsm.ServiceAssetStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "SimulateServiceAsset", (request.ServiceAsset?.ServiceAssetId ?? ""), token);
        return response;
    }
}
