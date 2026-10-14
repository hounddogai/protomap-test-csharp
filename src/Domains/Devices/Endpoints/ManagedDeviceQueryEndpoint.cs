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
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Answers read-only questions about managed devices, for screens and reports.
/// </summary>
public sealed class ManagedDeviceQueryEndpoint : ApiDevices.ManagedDeviceQueryService.ManagedDeviceQueryServiceBase
{
    private readonly ApiAudit.AuditEventService.AuditEventServiceClient _auditAuditEventService;
    private readonly ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient _devicesComplianceScanCalculationService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ILogger<ManagedDeviceQueryEndpoint> _logger;
    private readonly ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient _maintenanceWorkOrderQueryService;
    private readonly ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient _metricsMetricSeriesQueryService;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDevices.ScoreManagedDeviceRequest, CallOptions, AsyncUnaryCall<ApiDevices.ScoreManagedDeviceResponse>> _sendSuggestManagedDevices0;

    public ManagedDeviceQueryEndpoint(
        ApiAudit.AuditEventService.AuditEventServiceClient auditAuditEventService,
        ApiDevices.ComplianceScanCalculationService.ComplianceScanCalculationServiceClient devicesComplianceScanCalculationService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ILogger<ManagedDeviceQueryEndpoint> logger,
        ApiMaintenance.WorkOrderQueryService.WorkOrderQueryServiceClient maintenanceWorkOrderQueryService,
        ApiMetrics.MetricSeriesQueryService.MetricSeriesQueryServiceClient metricsMetricSeriesQueryService,
        IRecordRepository repository)
    {
        _auditAuditEventService = auditAuditEventService;
        _devicesComplianceScanCalculationService = devicesComplianceScanCalculationService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _logger = logger;
        _maintenanceWorkOrderQueryService = maintenanceWorkOrderQueryService;
        _metricsMetricSeriesQueryService = metricsMetricSeriesQueryService;
        _repository = repository;
        _sendSuggestManagedDevices0 = _devicesManagedDeviceValidationService.ScoreManagedDeviceAsync;
    }
    /// <summary>
    /// Lists managed devices a page at a time.
    /// </summary>
    public override async Task<ApiDevices.BrowseManagedDevicesResponse> BrowseManagedDevices(ApiDevices.BrowseManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BrowseManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "BrowseManagedDevices", "accepted", token);
        var input0 = new ApiMetrics.BrowseMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiMetrics.BrowseMetricSeriesesRequest>();
        using var rpc0 = _metricsMetricSeriesQueryService.BrowseMetricSeriesesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.NormalizeManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiDevices.NormalizeManagedDeviceRequest>(json1);
        using var rpc1 = _devicesManagedDeviceValidationService.NormalizeManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.BrowseManagedDevicesResponse
        {
            ManagedDevices = { new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
            } },
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply0.PageInfo?.NextPageToken ?? ""),
                TotalSize = (reply0.PageInfo?.TotalSize ?? 0L),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "BrowseManagedDevices", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts managed devices by status.
    /// </summary>
    public override async Task<ApiDevices.CountManagedDevicesResponse> CountManagedDevices(ApiDevices.CountManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CountManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "CountManagedDevices", "accepted", token);
        var response = new ApiDevices.CountManagedDevicesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "CountManagedDevices", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching managed device.
    /// </summary>
    public override async Task ExportManagedDevices(ApiDevices.ExportManagedDevicesRequest request, IServerStreamWriter<ApiDevices.ExportManagedDevicesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExportManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "ExportManagedDevices", "accepted", token);
        var input0 = new ApiDocuments.PreviewSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PreviewSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDocuments.SignatureMeasurement
                {
                    ObservedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                } },
            },
        };
        using var rpc0 = _documentsSignatureValidationService.PreviewSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.NormalizeEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeEntitlementRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementValidationService.NormalizeEntitlementAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMaintenance.ResolveWorkOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _maintenanceWorkOrderQueryService.ResolveWorkOrdersAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.ExportManagedDevicesResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = (reply1.Entitlement?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ExportManagedDevices", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists managed devices a page at a time.
    /// </summary>
    public override async Task<ApiDevices.FindManagedDevicesResponse> FindManagedDevices(ApiDevices.FindManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling FindManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "FindManagedDevices", "accepted", token);
        var response = new ApiDevices.FindManagedDevicesResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "FindManagedDevices", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a managed device.
    /// </summary>
    public override async Task<ApiDevices.LookupManagedDeviceResponse> LookupManagedDevice(ApiDevices.LookupManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ManagedDeviceId, "managed_device_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling LookupManagedDevice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", request.ManagedDeviceId,
            "LookupManagedDevice", "accepted", token);
        var input0 = new ApiDevices.CalculateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Source = (request.ManagedDeviceId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CalculateComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.LookupManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a managed device.
    /// </summary>
    public override async Task<ApiDevices.PeekManagedDeviceResponse> PeekManagedDevice(ApiDevices.PeekManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ManagedDeviceId, "managed_device_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PeekManagedDevice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", request.ManagedDeviceId,
            "PeekManagedDevice", "accepted", token);
        var input0 = new ApiAudit.CloneAuditEventRequest
        {
            AuditEvent = new ApiAudit.AuditEvent
            {
                CorrelationId = (request.ManagedDeviceId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiAudit.AuditEventMeasurement
                {
                    ObservedAt = (request.Context?.Deadline ?? new Timestamp()),
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _auditAuditEventService.CloneAuditEventAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.PeekManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.AuditEvent?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists managed devices a page at a time.
    /// </summary>
    public override async Task<ApiDevices.SuggestManagedDevicesResponse> SuggestManagedDevices(ApiDevices.SuggestManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SuggestManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "SuggestManagedDevices", "accepted", token);
        var input0 = new ApiDevices.ScoreManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ScoreManagedDeviceRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendSuggestManagedDevices0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.SuggestManagedDevicesResponse
        {
            ManagedDevices = { new ApiDevices.ManagedDevice
            {
                Dimensions = new ApiDevices.ManagedDeviceDimensions
                {
                    Length = (reply0.ManagedDevice?.Dimensions?.Length ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "SuggestManagedDevices", (request.Page?.PageToken ?? ""), token);
        return response;
    }
}
