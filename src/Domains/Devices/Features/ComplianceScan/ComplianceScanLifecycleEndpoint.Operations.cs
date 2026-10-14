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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

public sealed partial class ComplianceScanLifecycleEndpoint
{
    /// <summary>
    /// Activates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ActivateComplianceScanResponse> ActivateComplianceScan(ApiDevices.ActivateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling ActivateComplianceScan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "ActivateComplianceScan", "accepted", token);
        var input0 = new ApiRecruiting.CompareJobOpeningOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _recruitingJobOpeningCalculationService.CompareJobOpeningOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ExportManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.ExportManagedDevicesRequest.Types.Filter
            {
                Region = (reply0.JobOpening?.BillingAddress?.Region ?? ""),
            },
        };
        using var stream1 = _devicesManagedDeviceQueryService.ExportManagedDevices(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportManagedDevices frame {Type}", change1.GetType().Name);
        }
        var input2 = new ApiRecruiting.CheckJobOpeningEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.JobOpening?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.JobOpening?.BillingAddress?.PostalCode ?? ""),
                },
                Card = new ApiRecruiting.JobOpeningCardSettlement
                {
                    Token = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Card => (reply0.JobOpening?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _recruitingJobOpeningValidationService.CheckJobOpeningEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.ActivateComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
            Outcome = new ApiDevices.ActivateComplianceScanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ActivateComplianceScan", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ExpireComplianceScanResponse> ExpireComplianceScan(ApiDevices.ExpireComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling ExpireComplianceScan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "ExpireComplianceScan", "accepted", token);
        var input0 = new ApiDevices.ValidateManagedDeviceRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiDevices.ValidateManagedDeviceRequest.Types.Details
        {
            Contact = new ApiCommon.ContactInfo
            {
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
            },
        };
        input0.ManagedDevice = new ApiDevices.ManagedDevice
        {
            Owner = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _devicesManagedDeviceValidationService.ValidateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.SummarizeDevicePoliciesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SummarizeDevicePoliciesRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _devicesDevicePolicyQueryService.SummarizeDevicePoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.NormalizeComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.NormalizeComplianceScanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanValidationService.NormalizeComplianceScanAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.PreviewManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ManagedDevice?.Audit?.Etag ?? ""),
                },
                Components = { { "primary", new ApiDevices.ManagedDeviceComponent
                {
                    Description = (reply0.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var reply3 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesManagedDeviceValidationService.PreviewManagedDeviceAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.ExpireComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (reply2.ComplianceScan?.ComplianceScanId ?? ""),
                OccurredAt = (reply2.ComplianceScan?.OccurredAt ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Outcome = new ApiDevices.ExpireComplianceScanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply3.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ExpireComplianceScan", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Reinstates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ReinstateComplianceScanResponse> ReinstateComplianceScan(ApiDevices.ReinstateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling ReinstateComplianceScan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "ReinstateComplianceScan", "accepted", token);
        var input0 = new ApiBenefits.WatchBenefitPlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
                Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
            },
        };
        using var stream0 = _benefitsBenefitPlanService.WatchBenefitPlans(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchBenefitPlans frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiRecruiting.UpdateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                },
                Counterparty = new ApiRecruiting.InterviewRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    StartsAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Lines = { new ApiRecruiting.JobOpeningLine
                {
                    Attributes = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc1 = _sendReinstateComplianceScan1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ReinstateComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
            Outcome = new ApiDevices.ReinstateComplianceScanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ReinstateComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ScheduleComplianceScanExpiryResponse> ScheduleComplianceScanExpiry(ApiDevices.ScheduleComplianceScanExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleComplianceScanExpiry for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.StartSoftwareLicenseWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.StartSoftwareLicenseWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceClient>("acme.licensing.v1.SoftwareLicenseWorkflowService");
        using var rpc0 = named0.StartSoftwareLicenseWorkflowAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ShareDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareDocumentFolderExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                    PreferredChannel = (reply0.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentFolderExportService.ShareDocumentFolderExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ScheduleComplianceScanExpiryResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            Outcome = new ApiDevices.ScheduleComplianceScanExpiryResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ScheduleComplianceScanExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.SuspendComplianceScanResponse> SuspendComplianceScan(ApiDevices.SuspendComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling SuspendComplianceScan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "SuspendComplianceScan", "accepted", token);
        var input0 = new ApiRecruiting.CountCandidatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.CountCandidatesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.CountCandidatesRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingCandidateQueryService.CountCandidatesAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.EstimateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _devicesComplianceScanCalculationService.EstimateComplianceScanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.RecalculateCandidatesRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                CandidateId = (reply1.ComplianceScan?.Subject?.CandidateId ?? ""),
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _recruitingCandidateCalculationService.RecalculateCandidatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.SuspendComplianceScanToSearchJobOpenings3(reply0, reply2);
        using var rpc3 = _recruitingJobOpeningService.SearchJobOpeningsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiRecruiting.SearchJobOpeningsRequest>("acme.devices.v1.ComplianceScanLifecycleService/SuspendComplianceScan/3", input3.Clone()), token);
        var response = new ApiDevices.SuspendComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply1.ComplianceScan?.CorrelationId ?? ""),
            },
            Outcome = new ApiDevices.SuspendComplianceScanResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "SuspendComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.TerminateComplianceScanResponse> TerminateComplianceScan(ApiDevices.TerminateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling TerminateComplianceScan for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "TerminateComplianceScan", "accepted", token);
        var input0 = new ApiDevices.CreateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var reply0 = await SendTerminateComplianceScan0Async(input0, token);
        var input1 = new ApiLedger.ExplainLedgerAccountDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.ExplainLedgerAccountDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _ledgerLedgerAccountValidationService.ExplainLedgerAccountDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.TerminateComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiDevices.TerminateComplianceScanResponse.Types.Outcome
            {
                EffectiveAt = (reply1.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "TerminateComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.CreateManagedDeviceResponse> SendTerminateComplianceScan0Async(ApiDevices.CreateManagedDeviceRequest request, CancellationToken token)
    {
        return await _complianceScanLifecycleEndpointSendTerminateComplianceScan0Port.SendAsync(request, token);
    }
}
