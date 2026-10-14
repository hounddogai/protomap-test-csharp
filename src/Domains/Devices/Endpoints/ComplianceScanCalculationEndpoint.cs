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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Computes the figures of a compliance scan, such as totals, taxes, and estimates.
/// </summary>
public sealed class ComplianceScanCalculationEndpoint : ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiCareplans.CarePlanService.CarePlanServiceClient _careplansCarePlanService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiIdentity.UserService.UserServiceClient _identityUserService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<ComplianceScanCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient _metricsMetricSeriesValidationService;
    private readonly RetryPolicy _policy;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.RecalculateManagedDevicesRequest, CallOptions, AsyncUnaryCall<ApiDevices.RecalculateManagedDevicesResponse>> _sendEstimateComplianceScan1;
    private readonly IOptions<MeshOptions> _settings;

    public ComplianceScanCalculationEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiCareplans.CarePlanService.CarePlanServiceClient careplansCarePlanService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiIdentity.UserService.UserServiceClient identityUserService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<ComplianceScanCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesValidationService.MetricSeriesValidationServiceClient metricsMetricSeriesValidationService,
        RetryPolicy policy,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _auditAuditEventService = auditAuditEventService;
        _careplansCarePlanService = careplansCarePlanService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _identityUserService = identityUserService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _metricsMetricSeriesValidationService = metricsMetricSeriesValidationService;
        _policy = policy;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendEstimateComplianceScan1 = _devicesManagedDeviceCalculationService.RecalculateManagedDevicesAsync;
    }
    /// <summary>
    /// Calculates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.CalculateComplianceScanResponse> CalculateComplianceScan(ApiDevices.CalculateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SimulateEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiLicensing.SimulateEntitlementRequest>(json0);
        using var rpc0 = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SubscribeDocumentUpdatesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SubscribeDocumentUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Block => (reply0.Entitlement?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
                Variables = { (request.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
            {
                ApiLicensing.Entitlement.ActionOneofCase.Block => (reply0.Entitlement?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc1 = _documentsDocumentNotificationService.SubscribeDocumentUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.CreateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Entitlement?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply1.Document?.BillingAddress?.PostalCode ?? ""),
                    Recipient = (reply1.Document?.BillingAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc2 = _identityUserService.CreateUserAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.BrowseSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.BrowseSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (reply1.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseQueryService.BrowseSoftwareLicensesAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.CalculateComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Attributes = { (reply2.User?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                Subject = (request.ComplianceScan?.Subject ?? new ApiRecruiting.CandidateRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "CalculateComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.CompareComplianceScanOptionsResponse> CompareComplianceScanOptions(ApiDevices.CompareComplianceScanOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareComplianceScanOptions for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiMetrics.PreviewMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesValidationService.PreviewMetricSeriesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.CompareComplianceScanOptionsResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Source = (request.ComplianceScan?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "CompareComplianceScanOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.EstimateComplianceScanResponse> EstimateComplianceScan(ApiDevices.EstimateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SimulateEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingEntitlementCalculationService.SimulateEntitlementAsync);
        var input1 = new ApiDevices.RecalculateManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateManagedDevicesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _sendEstimateComplianceScan1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.EstimateComplianceScanResponse
        {
            Breakdown = new ApiDevices.EstimateComplianceScanResponse.Types.Breakdown
            {
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    Unit = (reply1.ManagedDevice?.Dimensions?.Unit ?? ""),
                } },
                OccurredAt = (request.ComplianceScan?.OccurredAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "EstimateComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ExplainComplianceScanCalculationResponse> ExplainComplianceScanCalculation(ApiDevices.ExplainComplianceScanCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainComplianceScanCalculation for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CountSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.CountSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (request.ComplianceScan?.OccurredAt ?? new Timestamp()),
            },
        };
        var named0 = _registry.Get<ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient>("acme.licensing.v1.SoftwareLicenseQueryService");
        using var rpc0 = named0.CountSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var batch1 = (request.ComplianceScan?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiDevices.ComplianceScanMeasurement>()).Select(async item =>
        {
            var input1 = new ApiDevices.CalculateManagedDeviceRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                ManagedDevice = new ApiDevices.ManagedDevice
                {
                    Dimensions = new ApiDevices.ManagedDeviceDimensions
                    {
                        Unit = (item.Unit ?? ""),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                        Email = (request.Details?.Contact?.Email ?? ""),
                    },
                },
            };
            using var rpc1 = _devicesManagedDeviceCalculationService.CalculateManagedDeviceAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            return reply1;
        }).ToArray();
        var replies1 = await Task.WhenAll(batch1);
        var reply1 = replies1.LastOrDefault() ?? new ApiDevices.CalculateManagedDeviceResponse();
        var response = new ApiDevices.ExplainComplianceScanCalculationResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Source = (request.ComplianceScan?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ExplainComplianceScanCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.GetComplianceScanRatesResponse> GetComplianceScanRates(ApiDevices.GetComplianceScanRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetComplianceScanRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "GetComplianceScanRates", "accepted", token);
        var input0 = new ApiLicensing.CompareEntitlementOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    NotifyEmail = (request.Context?.Actor?.Email ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.CompareEntitlementOptionsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingEntitlementCalculationService.CompareEntitlementOptionsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiDevices.GetComplianceScanRatesRequest, ApiLicensing.FindSoftwareLicensesRequest>(value => new ApiLicensing.FindSoftwareLicensesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.FindSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Entitlement?.EntitlementId ?? ""),
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _licensingSoftwareLicenseQueryService.FindSoftwareLicensesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.GetComplianceScanRatesResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Recalculates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.RecalculateComplianceScansResponse> RecalculateComplianceScans(ApiDevices.RecalculateComplianceScansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateComplianceScans for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ValidateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCareplans.ListCarePlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCareplans.ListCarePlansRequest.Types.Filter
            {
                CreatedAfter = (reply0.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _careplansCarePlanService.ListCarePlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var reply2 = new ApiAudit.UpdateAuditEventResponse();
        foreach (var item in (request.ComplianceScan?.Measurements ?? new Google.Protobuf.Collections.RepeatedField<ApiDevices.ComplianceScanMeasurement>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiAudit.UpdateAuditEventRequest
            {
                AuditEvent = new ApiAudit.AuditEvent
                {
                    Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                    Measurements = { new ApiAudit.AuditEventMeasurement
                    {
                        Metric = (item.Metric ?? ""),
                        ObservedAt = (item.ObservedAt ?? new Timestamp()),
                        Unit = (item.Unit ?? ""),
                        Value = item.Value,
                    } },
                },
                Context = (request.Context ?? new ApiCommon.RequestContext()),
            };
            using var rpc2 = _auditAuditEventService.UpdateAuditEventAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiDevices.RecalculateComplianceScansResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Source = (request.ComplianceScan?.Source ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "RecalculateComplianceScans", (request.Reason ?? ""), token);
        return response;
    }
}
