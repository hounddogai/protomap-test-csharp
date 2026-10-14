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
using ApiAppointments = Acme.Appointments.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiForecasting = Acme.Forecasting.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPatients = Acme.Patients.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

public sealed partial class ComplianceScanValidationEndpoint
{
    /// <summary>
    /// Checks a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.CheckComplianceScanEligibilityResponse> CheckComplianceScanEligibility(ApiDevices.CheckComplianceScanEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckComplianceScanEligibility for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.NormalizeDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.NormalizeDevicePolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDevices.NormalizeDevicePolicyRequest>();
        using var rpc0 = _devicesDevicePolicyValidationService.NormalizeDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.SummarizeAccessPoliciesRequest.Types.Details
            {
                Period = (reply0.DevicePolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (request.Reason ?? ""),
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiAuthorization.SummarizeAccessPoliciesRequest>(json1);
        using var rpc1 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.CheckComplianceScanEligibilityResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
            },
            Outcome = new ApiDevices.CheckComplianceScanEligibilityResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "CheckComplianceScanEligibility", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ExplainComplianceScanDecisionResponse> ExplainComplianceScanDecision(ApiDevices.ExplainComplianceScanDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainComplianceScanDecision for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPatients.CalculatePatientRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Patient = new ApiPatients.Patient
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Contact?.Email ?? ""),
                Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
            },
            RelationshipManager = new ApiIdentity.UserRef
            {
                DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
            },
        };
        using var rpc0 = _patientsPatientCalculationService.CalculatePatientAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ExplainComplianceScanDecisionResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ExplainComplianceScanDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.GetComplianceScanRulesResponse> GetComplianceScanRules(ApiDevices.GetComplianceScanRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ComplianceScanId, "compliance_scan_id");
        _logger.LogInformation("Handling GetComplianceScanRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan", request.ComplianceScanId,
            "GetComplianceScanRules", "accepted", token);
        var response = new ApiDevices.GetComplianceScanRulesResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.LintComplianceScansResponse> LintComplianceScans(ApiDevices.LintComplianceScansRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintComplianceScans for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    CandidateId = (request.ComplianceScan?.Subject?.CandidateId ?? ""),
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.BatchGetDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _documentsDocumentService.BatchGetDocumentsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.LintComplianceScansResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Source = (reply0.ComplianceScan?.Source ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "LintComplianceScans", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.NormalizeComplianceScanResponse> NormalizeComplianceScan(ApiDevices.NormalizeComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.SummarizeSoftwareLicensesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SummarizeSoftwareLicensesRequest.Types.Details
            {
                Filters = { (request.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _licensingSoftwareLicenseQueryService.SummarizeSoftwareLicensesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiDevices.RecalculateManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesManagedDeviceCalculationService.RecalculateManagedDevicesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiCareplans.CompareCarePlanOptionsRequest
        {
            CarePlan = new ApiCareplans.CarePlan
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply1.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Region = (reply1.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply1.ManagedDevice?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiAppointments.AppointmentRef
                {
                    DisplayName = (reply1.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc2 = _careplansCarePlanCalculationService.CompareCarePlanOptionsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.RecalculateComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc3 = _devicesComplianceScanCalculationService.RecalculateComplianceScansAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.NormalizeComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
            Outcome = new ApiDevices.NormalizeComplianceScanResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Locale ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "NormalizeComplianceScan", (request.Details?.Locale ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.PreviewComplianceScanResponse> PreviewComplianceScan(ApiDevices.PreviewComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ScoreLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Attributes = { (request.ComplianceScan?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var descriptor0 = new Method<ApiLicensing.ScoreLicenseAssignmentRequest, ApiLicensing.ScoreLicenseAssignmentResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".LicenseAssignmentValidationService", "ScoreLicenseAssignment",
            Wire.Marshaller<ApiLicensing.ScoreLicenseAssignmentRequest>(), Wire.Marshaller<ApiLicensing.ScoreLicenseAssignmentResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ScoreDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ScoreDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.LicenseAssignment?.BillingAddress?.PostalCode ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiDevices.ScoreDevicePolicyRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _devicesDevicePolicyValidationService.ScoreDevicePolicyAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.PreviewComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Source = (request.ComplianceScan?.Source ?? ""),
            },
            Outcome = new ApiDevices.PreviewComplianceScanResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "PreviewComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ScoreComplianceScanResponse> ScoreComplianceScan(ApiDevices.ScoreComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiDevices.ScoreComplianceScanRequest, ApiDevices.QuoteManagedDeviceRequest>(value => new ApiDevices.QuoteManagedDeviceRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.QuoteManagedDeviceRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (value.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Recipient = (value.Details?.Destination?.Recipient ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (value.Details?.Destination?.PostalCode ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _devicesManagedDeviceCalculationService.QuoteManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiForecasting.UpdateDemandForecastRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DemandForecast = new ApiForecasting.DemandForecast
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiForecasting.ScenarioRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _forecastingDemandForecastService.UpdateDemandForecastAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ScoreComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                CorrelationId = (request.ComplianceScan?.CorrelationId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ScoreComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a compliance scan.
    /// </summary>
    public override async Task<ApiDevices.ValidateComplianceScanResponse> ValidateComplianceScan(ApiDevices.ValidateComplianceScanRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateComplianceScan for {Tenant}", requestContext.TenantId);
        if (request.ComplianceScan is { } record)
        {
            var snapshot = ComplianceScanMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.ValidateComplianceScanToSuggestEntitlements0();
        using var rpc0 = _licensingEntitlementQueryService.SuggestEntitlementsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.SimulateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (request.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        var reply1 = await SendValidateComplianceScan1Async(input1, token);
        var response = new ApiDevices.ValidateComplianceScanResponse
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                ComplianceScanId = (request.ComplianceScanId ?? ""),
                Sequence = (request.ComplianceScan?.Sequence ?? 0L),
            },
            Outcome = new ApiDevices.ValidateComplianceScanResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "compliance_scan",
            "audit", "ValidateComplianceScan", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.SimulateManagedDeviceResponse> SendValidateComplianceScan1Async(ApiDevices.SimulateManagedDeviceRequest request, CancellationToken token)
    {
        return await _complianceScanValidationEndpointSendValidateComplianceScan1Port.SendAsync(request, token);
    }
}
