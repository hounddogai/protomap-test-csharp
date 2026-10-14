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
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Computes the figures of a problem, such as totals, taxes, and estimates.
/// </summary>
public sealed class ProblemCalculationEndpoint : ApiItsm.ProblemCalculationService.ProblemCalculationServiceBase
{
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient _itsmIncidentCalculationService;
    private readonly ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient _itsmServiceAssetCalculationService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient _licensingLicenseAssignmentValidationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ILogger<ProblemCalculationEndpoint> _logger;
    private readonly ApiMetrics.MetricSeriesService.MetricSeriesServiceClient _metricsMetricSeriesService;
    private readonly IProblemCalculationEndpointSendEstimateProblem1Port _problemCalculationEndpointSendEstimateProblem1Port;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiItsm.EstimateServiceAssetRequest, CallOptions, AsyncUnaryCall<ApiItsm.EstimateServiceAssetResponse>> _sendSimulateProblem0;
    private readonly IOptions<MeshOptions> _settings;

    public ProblemCalculationEndpoint(
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        CallInvoker invoker,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.IncidentCalculationService.IncidentCalculationServiceClient itsmIncidentCalculationService,
        ApiItsm.ServiceAssetCalculationService.ServiceAssetCalculationServiceClient itsmServiceAssetCalculationService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLicensing.LicenseAssignmentValidationService.LicenseAssignmentValidationServiceClient licensingLicenseAssignmentValidationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ILogger<ProblemCalculationEndpoint> logger,
        ApiMetrics.MetricSeriesService.MetricSeriesServiceClient metricsMetricSeriesService,
        IProblemCalculationEndpointSendEstimateProblem1Port problemCalculationEndpointSendEstimateProblem1PortAdapter,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _invoker = invoker;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmIncidentCalculationService = itsmIncidentCalculationService;
        _itsmServiceAssetCalculationService = itsmServiceAssetCalculationService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _licensingLicenseAssignmentValidationService = licensingLicenseAssignmentValidationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _logger = logger;
        _metricsMetricSeriesService = metricsMetricSeriesService;
        _problemCalculationEndpointSendEstimateProblem1Port = problemCalculationEndpointSendEstimateProblem1PortAdapter;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendSimulateProblem0 = _itsmServiceAssetCalculationService.EstimateServiceAssetAsync;
    }
    /// <summary>
    /// Calculates a problem.
    /// </summary>
    public override async Task<ApiItsm.CalculateProblemResponse> CalculateProblem(ApiItsm.CalculateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SummarizeSoftwareLicensesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var named0 = _registry.Get<ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient>("acme.licensing.v1.SoftwareLicenseQueryService");
        using var rpc0 = named0.SummarizeSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.NormalizeLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Problem?.Assignee?.Email ?? ""),
                    PreferredChannel = (request.Problem?.Requester?.PreferredChannel ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
        };
        var descriptor1 = new Method<ApiLicensing.NormalizeLicenseAssignmentRequest, ApiLicensing.NormalizeLicenseAssignmentResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".LicenseAssignmentValidationService", "NormalizeLicenseAssignment",
            Wire.Marshaller<ApiLicensing.NormalizeLicenseAssignmentRequest>(), Wire.Marshaller<ApiLicensing.NormalizeLicenseAssignmentResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiMetrics.UpdateMetricSeriesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            MetricSeries = new ApiMetrics.MetricSeries
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiMetrics.UpdateMetricSeriesRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _metricsMetricSeriesService.UpdateMetricSeriesAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiItsm.CalculateProblemResponse
        {
            Breakdown = new ApiItsm.CalculateProblemResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply2.MetricSeries?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Problem?.Subject ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "CalculateProblem", (request.Problem?.Subject ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a problem.
    /// </summary>
    public override async Task<ApiItsm.CompareProblemOptionsResponse> CompareProblemOptions(ApiItsm.CompareProblemOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareProblemOptions for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.SimulateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Problem?.Audit?.Etag ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (request.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _itsmChangeRequestCalculationService.SimulateChangeRequestAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var reply1 = new ApiItsm.EstimateIncidentResponse();
        foreach (var item in (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input1 = new ApiItsm.EstimateIncidentRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Incident = new ApiItsm.Incident
                {
                    Comments = { new ApiItsm.IncidentComment
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                        Visibility = (item.Visibility ?? ""),
                    } },
                },
            };
            using var rpc1 = _itsmIncidentCalculationService.EstimateIncidentAsync(input1, cancellationToken: token);
            reply1 = await rpc1.ResponseAsync;
        }
        var response = new ApiItsm.CompareProblemOptionsResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ChangeRequest?.Audit?.Etag ?? ""),
                },
                CustomFields = { (request.Problem?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ProblemId = (request.ProblemId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "CompareProblemOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a problem.
    /// </summary>
    public override async Task<ApiItsm.EstimateProblemResponse> EstimateProblem(ApiItsm.EstimateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.EstimateProblemToCalculateManagedDevice0();
        using var rpc0 = _devicesManagedDeviceCalculationService.CalculateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.EstimateIncidentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.EstimateIncidentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Problem?.Assignee?.Email ?? ""),
                    UserId = (reply0.ManagedDevice?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await SendEstimateProblem1Async(input1, token);
        var reply2 = new ApiLicensing.RecalculateLicenseAssignmentsResponse();
        foreach (var item in (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input2 = new ApiLicensing.RecalculateLicenseAssignmentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiLicensing.RecalculateLicenseAssignmentsRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                        Region = (request.Details?.Destination?.Region ?? ""),
                    },
                },
                LicenseAssignment = new ApiLicensing.LicenseAssignment
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Counterparty = new ApiLicensing.EntitlementRef
                    {
                        DisplayName = (request.Problem?.Assignee?.DisplayName ?? ""),
                    },
                },
            };
            using var rpc2 = _licensingLicenseAssignmentCalculationService.RecalculateLicenseAssignmentsAsync(input2, cancellationToken: token);
            reply2 = await rpc2.ResponseAsync;
        }
        var response = new ApiItsm.EstimateProblemResponse
        {
            Breakdown = new ApiItsm.EstimateProblemResponse.Types.Breakdown
            {
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Incident?.Audit?.CreatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Sla = new ApiItsm.ProblemSla
                {
                    PausedFor = (reply1.Incident?.Sla?.PausedFor ?? new Duration()),
                },
                Subject = (request.Problem?.Subject ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "EstimateProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a problem.
    /// </summary>
    public override async Task<ApiItsm.ExplainProblemCalculationResponse> ExplainProblemCalculation(ApiItsm.ExplainProblemCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainProblemCalculation for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.ExplainServiceAssetCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Problem?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    Email = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _itsmServiceAssetCalculationService.ExplainServiceAssetCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMetrics.WatchMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Phone = (request.Details?.Contact?.Phone ?? ""),
                PreferredChannel = (request.Problem?.Requester?.PreferredChannel ?? ""),
            },
        };
        using var stream1 = _metricsMetricSeriesService.WatchMetricSerieses(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchMetricSerieses frame {Type}", change1.GetType().Name);
        }
        var response = new ApiItsm.ExplainProblemCalculationResponse
        {
            Problem = new ApiItsm.Problem
            {
                Priority = (request.Problem?.Priority ?? 0),
                ProblemId = (request.ProblemId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "ExplainProblemCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a problem.
    /// </summary>
    public override async Task<ApiItsm.GetProblemRatesResponse> GetProblemRates(ApiItsm.GetProblemRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ProblemId, "problem_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetProblemRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "problem", request.ProblemId,
            "GetProblemRates", "accepted", token);
        var input0 = new ApiLicensing.SimulateEntitlementRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Entitlement = new ApiLicensing.Entitlement
        {
            Block = new ApiLicensing.EntitlementBlockAction
            {
                NotifyEmail = (request.Context?.Actor?.Email ?? ""),
            },
            Owner = new ApiIdentity.UserRef
            {
                UserId = (request.Context?.Actor?.UserId ?? ""),
            },
        };
        using var rpc0 = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.EstimateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
            {
                ApiLicensing.Entitlement.ActionOneofCase.Block => (reply0.Entitlement?.Block?.Reason ?? ""),
                _ => ""
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Entitlement?.Owner?.DisplayName ?? ""),
                    UserId = (reply0.Entitlement?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _itsmServiceAssetCalculationService.EstimateServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.GetProblemRatesResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (reply0.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                ProblemId = (request.ProblemId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Entitlement?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a problem.
    /// </summary>
    public override async Task<ApiItsm.QuoteProblemResponse> QuoteProblem(ApiItsm.QuoteProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.PreviewManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PreviewManagedDeviceRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Problem?.Requester?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Problem?.Requester?.PreferredChannel ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Problem?.Assignee?.UserId ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesManagedDeviceValidationService.PreviewManagedDeviceAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDevices.PreviewDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PreviewDevicePolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Problem ?? new ApiItsm.Problem()).OriginCase switch
                    {
                        ApiItsm.Problem.OriginOneofCase.Customer => (request.Problem?.Customer?.Email ?? ""),
                        _ => ""
                    },
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyValidationService.PreviewDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.QuoteProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "QuoteProblem", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a problem.
    /// </summary>
    public override async Task<ApiItsm.RecalculateProblemsResponse> RecalculateProblems(ApiItsm.RecalculateProblemsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateProblems for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.Problem?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiItsm.ProblemComment>()).Select(async item =>
        {
            var input0 = new ApiItsm.CalculateServiceAssetRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiItsm.CalculateServiceAssetRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Region = (request.Details?.Destination?.Region ?? ""),
                    },
                },
                ServiceAsset = new ApiItsm.ServiceAsset
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (item.CreatedAt ?? new Timestamp()),
                    },
                    Owner = new ApiIdentity.UserRef
                    {
                        Email = (item.AuthorEmail ?? ""),
                    },
                },
            };
            using var rpc0 = _itsmServiceAssetCalculationService.CalculateServiceAssetAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiItsm.CalculateServiceAssetResponse();
        var response = new ApiItsm.RecalculateProblemsResponse
        {
            Problem = new ApiItsm.Problem
            {
                Priority = (request.Problem?.Priority ?? 0),
                ProblemId = (request.ProblemId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "RecalculateProblems", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a problem.
    /// </summary>
    public override async Task<ApiItsm.SimulateProblemResponse> SimulateProblem(ApiItsm.SimulateProblemRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateProblem for {Tenant}", requestContext.TenantId);
        if (request.Problem is { } record)
        {
            var snapshot = ProblemMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.EstimateServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendSimulateProblem0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.SimulateProblemResponse
        {
            Problem = new ApiItsm.Problem
            {
                ProblemId = (request.ProblemId ?? ""),
                Status = (request.Problem?.Status ?? (ApiItsm.ProblemStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "problem",
            "audit", "SimulateProblem", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiItsm.EstimateIncidentResponse> SendEstimateProblem1Async(ApiItsm.EstimateIncidentRequest request, CancellationToken token)
    {
        return await _problemCalculationEndpointSendEstimateProblem1Port.SendAsync(request, token);
    }
}
