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
using ApiExperiments = Acme.Experiments.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Runs the long-lived workflow that takes a device policy from start to finish.
/// </summary>
public sealed class DevicePolicyWorkflowEndpoint : ApiDevices.DevicePolicyWorkflowService.DevicePolicyWorkflowServiceBase
{
    private readonly ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient _devicesDevicePolicyLifecycleService;
    private readonly ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient _devicesDevicePolicyQueryService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient _devicesManagedDeviceApprovalService;
    private readonly ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient _devicesManagedDeviceWorkflowService;
    private readonly ApiExperiments.ExperimentService.ExperimentServiceClient _experimentsExperimentService;
    private readonly CallInvoker _invoker;
    private readonly ApiLedger.LedgerAccountService.LedgerAccountServiceClient _ledgerLedgerAccountService;
    private readonly ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient _licensingEntitlementWorkflowService;
    private readonly ILogger<DevicePolicyWorkflowEndpoint> _logger;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiRecruiting.UpdateJobOpeningRequest, CallOptions, AsyncUnaryCall<ApiRecruiting.UpdateJobOpeningResponse>> _sendRetryDevicePolicyWorkflow0;
    private readonly IOptions<MeshOptions> _settings;

    public DevicePolicyWorkflowEndpoint(
        ApiDevices.DevicePolicyLifecycleService.DevicePolicyLifecycleServiceClient devicesDevicePolicyLifecycleService,
        ApiDevices.DevicePolicyQueryService.DevicePolicyQueryServiceClient devicesDevicePolicyQueryService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceApprovalService.ManagedDeviceApprovalServiceClient devicesManagedDeviceApprovalService,
        ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient devicesManagedDeviceWorkflowService,
        ApiExperiments.ExperimentService.ExperimentServiceClient experimentsExperimentService,
        CallInvoker invoker,
        ApiLedger.LedgerAccountService.LedgerAccountServiceClient ledgerLedgerAccountService,
        ApiLicensing.EntitlementWorkflowService.EntitlementWorkflowServiceClient licensingEntitlementWorkflowService,
        ILogger<DevicePolicyWorkflowEndpoint> logger,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _devicesDevicePolicyLifecycleService = devicesDevicePolicyLifecycleService;
        _devicesDevicePolicyQueryService = devicesDevicePolicyQueryService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceApprovalService = devicesManagedDeviceApprovalService;
        _devicesManagedDeviceWorkflowService = devicesManagedDeviceWorkflowService;
        _experimentsExperimentService = experimentsExperimentService;
        _invoker = invoker;
        _ledgerLedgerAccountService = ledgerLedgerAccountService;
        _licensingEntitlementWorkflowService = licensingEntitlementWorkflowService;
        _logger = logger;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _sendRetryDevicePolicyWorkflow0 = _recruitingJobOpeningService.UpdateJobOpeningAsync;
    }
    /// <summary>
    /// Advances a device policy.
    /// </summary>
    public override async Task<ApiDevices.AdvanceDevicePolicyWorkflowResponse> AdvanceDevicePolicyWorkflow(ApiDevices.AdvanceDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SuspendDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SuspendDevicePolicyRequest.Types.Details
            {
                NotifyContact = new ApiCommon.ContactInfo
                {
                    Email = (request.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
            DevicePolicyId = (request.DevicePolicyId ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _devicesDevicePolicyLifecycleService.SuspendDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.AdvanceDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Description = (request.DevicePolicy?.Description ?? ""),
                DevicePolicyId = (reply0.DevicePolicy?.DevicePolicyId ?? ""),
                Priority = (request.DevicePolicy?.Priority ?? 0),
            },
            Outcome = new ApiDevices.AdvanceDevicePolicyWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply0.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "AdvanceDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a device policy.
    /// </summary>
    public override async Task<ApiDevices.CancelDevicePolicyWorkflowResponse> CancelDevicePolicyWorkflow(ApiDevices.CancelDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ListPendingManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ListPendingManagedDevicesRequest.Types.Filter
            {
                CreatedAfter = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.DevicePolicy?.DevicePolicyId ?? ""),
            },
        };
        using var rpc0 = _devicesManagedDeviceApprovalService.ListPendingManagedDevicesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.LookupDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicyId = (request.DevicePolicyId ?? ""),
        };
        using var rpc1 = _devicesDevicePolicyQueryService.LookupDevicePolicyAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiDevices.CancelDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Description = (reply1.DevicePolicy?.Description ?? ""),
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.DevicePolicy?.Owner?.Email ?? ""),
                },
                Parameters = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiDevices.CancelDevicePolicyWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "CancelDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.InspectDevicePolicyWorkflowResponse> InspectDevicePolicyWorkflow(ApiDevices.InspectDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "InspectDevicePolicyWorkflow", "accepted", token);
        var input0 = new ApiRecruiting.RecalculateInterviewsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Interview = new ApiRecruiting.Interview
        {
            Participants = { new ApiRecruiting.InterviewParticipant
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            } },
        };
        using var rpc0 = _recruitingInterviewCalculationService.RecalculateInterviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.SearchLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.SearchLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
                Region = (reply0.Interview?.Location?.Region ?? ""),
            },
        };
        using var rpc1 = _ledgerLedgerAccountService.SearchLedgerAccountsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.InspectDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Interview?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.Interview?.Audit?.UpdatedBy ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Pauses a device policy.
    /// </summary>
    public override async Task<ApiDevices.PauseDevicePolicyWorkflowResponse> PauseDevicePolicyWorkflow(ApiDevices.PauseDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.QuoteOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.DevicePolicy?.Audit?.CreatedBy ?? ""),
                },
                Lines = { new ApiRecruiting.OfferLine
                {
                    Description = (request.DevicePolicy?.Description ?? ""),
                    Tax = new ApiRecruiting.OfferLineTax
                    {
                        Amount = (request.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                        {
                            ApiDevices.DevicePolicy.ActionOneofCase.Discount => (request.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _recruitingOfferCalculationService.QuoteOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.PauseDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Parameters = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "PauseDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a device policy.
    /// </summary>
    public override async Task<ApiDevices.ResumeDevicePolicyWorkflowResponse> ResumeDevicePolicyWorkflow(ApiDevices.ResumeDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ScoreJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.DevicePolicy?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Tax = new ApiRecruiting.JobOpeningLineTax
                    {
                        Amount = (request.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                        {
                            ApiDevices.DevicePolicy.ActionOneofCase.Discount => (request.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _recruitingJobOpeningValidationService.ScoreJobOpeningAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.RecalculateInterviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.RecalculateInterviewsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
                },
            },
            Interview = new ApiRecruiting.Interview
            {
                Location = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Details?.Notify?.Email ?? ""),
                        Phone = (request.Details?.Notify?.Phone ?? ""),
                    },
                } },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _recruitingInterviewCalculationService.RecalculateInterviewsAsync);
        var response = new ApiDevices.ResumeDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Parameters = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ResumeDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a device policy.
    /// </summary>
    public override async Task<ApiDevices.RetryDevicePolicyWorkflowResponse> RetryDevicePolicyWorkflow(ApiDevices.RetryDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.UpdateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (request.DevicePolicy?.Audit?.CreatedBy ?? ""),
                    UpdatedBy = (request.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc0 = _sendRetryDevicePolicyWorkflow0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.CancelManagedDeviceWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                    UserId = (request.DevicePolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceClient>("acme.devices.v1.ManagedDeviceWorkflowService");
        using var rpc1 = named1.CancelManagedDeviceWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.StartEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.StartEntitlementWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var descriptor2 = new Method<ApiLicensing.StartEntitlementWorkflowRequest, ApiLicensing.StartEntitlementWorkflowResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementWorkflowService", "StartEntitlementWorkflow",
            Wire.Marshaller<ApiLicensing.StartEntitlementWorkflowRequest>(), Wire.Marshaller<ApiLicensing.StartEntitlementWorkflowResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.RetryDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.JobOpening?.Audit?.CreatedBy ?? ""),
                    Etag = (reply2.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply1.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Priority = (request.DevicePolicy?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "RetryDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a device policy.
    /// </summary>
    public override async Task<ApiDevices.SignalDevicePolicyWorkflowResponse> SignalDevicePolicyWorkflow(ApiDevices.SignalDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiExperiments.BatchGetExperimentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiExperiments.BatchGetExperimentsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _experimentsExperimentService.BatchGetExperimentsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiDevices.SignalDevicePolicyWorkflowRequest, ApiDevices.ResolveDevicePoliciesRequest>(value => new ApiDevices.ResolveDevicePoliciesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _devicesDevicePolicyQueryService.ResolveDevicePoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.SignalDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Parameters = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "SignalDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a device policy.
    /// </summary>
    public override async Task<ApiDevices.StartDevicePolicyWorkflowResponse> StartDevicePolicyWorkflow(ApiDevices.StartDevicePolicyWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartDevicePolicyWorkflow for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.CalculateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.DevicePolicy?.Audit?.Etag ?? ""),
                },
                Lines = { new ApiRecruiting.OfferLine
                {
                    Tax = new ApiRecruiting.OfferLineTax
                    {
                        Amount = (request.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                        {
                            ApiDevices.DevicePolicy.ActionOneofCase.Discount => (request.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _recruitingOfferCalculationService.CalculateOfferAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.PreviewDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Block = new ApiDevices.DevicePolicyBlockAction
                {
                    NotifyEmail = (request.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Block => (request.DevicePolicy?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyValidationService.PreviewDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.StartDevicePolicyWorkflowResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Parameters = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiDevices.StartDevicePolicyWorkflowResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "StartDevicePolicyWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
