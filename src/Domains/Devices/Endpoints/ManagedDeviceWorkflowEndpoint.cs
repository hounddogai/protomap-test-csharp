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
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

/// <summary>
/// Runs the long-lived workflow that takes a managed device from start to finish.
/// </summary>
public sealed class ManagedDeviceWorkflowEndpoint : ApiDevices.ManagedDeviceWorkflowService.ManagedDeviceWorkflowServiceBase
{
    private readonly ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient _devicesComplianceScanLifecycleService;
    private readonly ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient _devicesDevicePolicyCalculationService;
    private readonly ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient _devicesDevicePolicyValidationService;
    private readonly ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient _devicesManagedDeviceCalculationService;
    private readonly ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient _devicesManagedDeviceValidationService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient _ledgerLedgerAccountQueryService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient _licensingSoftwareLicenseWorkflowService;
    private readonly ILogger<ManagedDeviceWorkflowEndpoint> _logger;
    private readonly ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient _recruitingCandidateQueryService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient _recruitingJobOpeningQueryService;
    private readonly ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient _recruitingJobOpeningValidationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public ManagedDeviceWorkflowEndpoint(
        ApiDevices.ComplianceScanLifecycleService.ComplianceScanLifecycleServiceClient devicesComplianceScanLifecycleService,
        ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient devicesDevicePolicyCalculationService,
        ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient devicesDevicePolicyValidationService,
        ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient devicesManagedDeviceCalculationService,
        ApiDevices.ManagedDeviceValidationService.ManagedDeviceValidationServiceClient devicesManagedDeviceValidationService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiLedger.LedgerAccountQueryService.LedgerAccountQueryServiceClient ledgerLedgerAccountQueryService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient licensingSoftwareLicenseWorkflowService,
        ILogger<ManagedDeviceWorkflowEndpoint> logger,
        ApiRecruiting.CandidateQueryService.CandidateQueryServiceClient recruitingCandidateQueryService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningQueryService.JobOpeningQueryServiceClient recruitingJobOpeningQueryService,
        ApiRecruiting.JobOpeningValidationService.JobOpeningValidationServiceClient recruitingJobOpeningValidationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _devicesComplianceScanLifecycleService = devicesComplianceScanLifecycleService;
        _devicesDevicePolicyCalculationService = devicesDevicePolicyCalculationService;
        _devicesDevicePolicyValidationService = devicesDevicePolicyValidationService;
        _devicesManagedDeviceCalculationService = devicesManagedDeviceCalculationService;
        _devicesManagedDeviceValidationService = devicesManagedDeviceValidationService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _ledgerLedgerAccountQueryService = ledgerLedgerAccountQueryService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _licensingSoftwareLicenseWorkflowService = licensingSoftwareLicenseWorkflowService;
        _logger = logger;
        _recruitingCandidateQueryService = recruitingCandidateQueryService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningQueryService = recruitingJobOpeningQueryService;
        _recruitingJobOpeningValidationService = recruitingJobOpeningValidationService;
        _repository = repository;
        _settings = settings;
    }
    /// <summary>
    /// Advances a managed device.
    /// </summary>
    public override async Task<ApiDevices.AdvanceManagedDeviceWorkflowResponse> AdvanceManagedDeviceWorkflow(ApiDevices.AdvanceManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingSoftwareLicenseWorkflowService.PauseSoftwareLicenseWorkflowAsync);
        var input1 = new ApiLedger.ExportLedgerAccountsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLedger.ExportLedgerAccountsRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.ManagedDevice?.Address?.Region ?? ""),
            },
        };
        using var stream1 = _ledgerLedgerAccountQueryService.ExportLedgerAccounts(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportLedgerAccounts frame {Type}", change1.GetType().Name);
        }
        var response = new ApiDevices.AdvanceManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Name = (request.ManagedDevice?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "AdvanceManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a managed device.
    /// </summary>
    public override async Task<ApiDevices.CancelManagedDeviceWorkflowResponse> CancelManagedDeviceWorkflow(ApiDevices.CancelManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PreviewSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PreviewSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.PreviewSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ScheduleComplianceScanExpiryRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ScheduleComplianceScanExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanLifecycleService.ScheduleComplianceScanExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.CancelManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiDevices.CancelManagedDeviceWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "CancelManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a managed device.
    /// </summary>
    public override async Task<ApiDevices.InspectManagedDeviceWorkflowResponse> InspectManagedDeviceWorkflow(ApiDevices.InspectManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ManagedDeviceId, "managed_device_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", request.ManagedDeviceId,
            "InspectManagedDeviceWorkflow", "accepted", token);
        var response = new ApiDevices.InspectManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists managed devices a page at a time.
    /// </summary>
    public override async Task<ApiDevices.ListManagedDeviceWorkflowsResponse> ListManagedDeviceWorkflows(ApiDevices.ListManagedDeviceWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListManagedDeviceWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "ListManagedDeviceWorkflows", "accepted", token);
        var input0 = new ApiDevices.CalculateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CalculateManagedDeviceRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiDevices.CalculateManagedDeviceRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _devicesManagedDeviceCalculationService.CalculateManagedDeviceAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ListManagedDeviceWorkflowsResponse
        {
            ManagedDevices = { new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ManagedDevice?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ListManagedDeviceWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a managed device.
    /// </summary>
    public override async Task<ApiDevices.PauseManagedDeviceWorkflowResponse> PauseManagedDeviceWorkflow(ApiDevices.PauseManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiDevices.PauseManagedDeviceWorkflowRequest, ApiDevices.ValidateManagedDeviceRequest>(value => new ApiDevices.ValidateManagedDeviceRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (value.ManagedDevice?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (value.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.PauseManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Code = (reply0.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "PauseManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Resumes a managed device.
    /// </summary>
    public override async Task<ApiDevices.ResumeManagedDeviceWorkflowResponse> ResumeManagedDeviceWorkflow(ApiDevices.ResumeManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ResumeManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ValidateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.ValidateSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ResumeManagedDeviceWorkflowToScoreDevicePolicy1(reply0);
        using var rpc1 = _devicesDevicePolicyValidationService.ScoreDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.ExplainDevicePolicyCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                Route = new ApiDevices.DevicePolicyRouteAction
                {
                    Weight = (reply1.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Route => (reply1.DevicePolicy?.Route?.Weight ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc2 = _devicesDevicePolicyCalculationService.ExplainDevicePolicyCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.ResumeManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Capacity = (request.ManagedDevice?.Capacity ?? 0L),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Name = (reply1.DevicePolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
            Outcome = new ApiDevices.ResumeManagedDeviceWorkflowResponse.Types.Outcome
            {
                EffectiveAt = (reply2.Outcome?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ResumeManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a managed device.
    /// </summary>
    public override async Task<ApiDevices.RetryManagedDeviceWorkflowResponse> RetryManagedDeviceWorkflow(ApiDevices.RetryManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.ExportCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.ExportCandidatesRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.ManagedDevice?.Address?.Region ?? ""),
            },
        };
        using var stream0 = _recruitingCandidateQueryService.ExportCandidates(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportCandidates frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiLicensing.RecallSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiLicensing.RecallSoftwareLicenseRequest>();
        using var rpc1 = _licensingSoftwareLicenseApprovalService.RecallSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.EstimateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
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
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiRecruiting.EstimateInterviewRequest>(json2);
        using var rpc2 = _recruitingInterviewCalculationService.EstimateInterviewAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiRecruiting.SuggestJobOpeningsRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Filter = new ApiRecruiting.SuggestJobOpeningsRequest.Types.Filter
        {
            Labels = { (reply1.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        using var rpc3 = _recruitingJobOpeningQueryService.SuggestJobOpeningsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.RetryManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Outcome = new ApiDevices.RetryManagedDeviceWorkflowResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "RetryManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a managed device.
    /// </summary>
    public override async Task<ApiDevices.SignalManagedDeviceWorkflowResponse> SignalManagedDeviceWorkflow(ApiDevices.SignalManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ApproveEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _licensingEntitlementApprovalService.ApproveEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.SignalManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Capacity = (request.ManagedDevice?.Capacity ?? 0L),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "SignalManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a managed device.
    /// </summary>
    public override async Task<ApiDevices.StartManagedDeviceWorkflowResponse> StartManagedDeviceWorkflow(ApiDevices.StartManagedDeviceWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartManagedDeviceWorkflow for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ValidateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateDevicePolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyValidationService.ValidateDevicePolicyAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.CheckJobOpeningEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CheckJobOpeningEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Tax = new ApiRecruiting.JobOpeningLineTax
                    {
                        Amount = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                        {
                            ApiDevices.DevicePolicy.ActionOneofCase.Discount => (reply0.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.CheckJobOpeningEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.PauseSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Card = new ApiLicensing.SoftwareLicenseCardFunding
                {
                    Token = (reply1.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply1.JobOpening?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Limits = { new ApiLicensing.SoftwareLicenseLimit
                {
                    Amount = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Discount => (reply0.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc2 = _licensingSoftwareLicenseWorkflowService.PauseSoftwareLicenseWorkflowAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.StartManagedDeviceWorkflowResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.JobOpening?.Audit?.Etag ?? ""),
                },
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Outcome = new ApiDevices.StartManagedDeviceWorkflowResponse.Types.Outcome
            {
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "StartManagedDeviceWorkflow", (request.Reason ?? ""), token);
        return response;
    }
}
