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
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Devices;

public sealed partial class DevicePolicyValidationEndpoint
{
    /// <summary>
    /// Explains a device policy.
    /// </summary>
    public override async Task<ApiDevices.ExplainDevicePolicyDecisionResponse> ExplainDevicePolicyDecision(ApiDevices.ExplainDevicePolicyDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainDevicePolicyDecision for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.SummarizeAccessPoliciesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
            {
                ApiDevices.DevicePolicy.ActionOneofCase.Block => (request.DevicePolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _authorizationAccessPolicyQueryService.SummarizeAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ExplainDevicePolicyDecisionResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Priority = (request.DevicePolicy?.Priority ?? 0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ExplainDevicePolicyDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.GetDevicePolicyRulesResponse> GetDevicePolicyRules(ApiDevices.GetDevicePolicyRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        _logger.LogInformation("Handling GetDevicePolicyRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "GetDevicePolicyRules", "accepted", token);
        var input0 = new ApiDevices.RecalculateManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesManagedDeviceCalculationService.RecalculateManagedDevicesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.GetDevicePolicyRulesResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lints a device policy.
    /// </summary>
    public override async Task<ApiDevices.LintDevicePoliciesResponse> LintDevicePolicies(ApiDevices.LintDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling LintDevicePolicies for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.GetDevicePolicyRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicyId = (request.DevicePolicy?.DevicePolicyId ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _devicesDevicePolicyCalculationService.GetDevicePolicyRatesAsync);
        var input1 = new ApiDevices.EstimateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.DevicePolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        using var rpc1 = _sendLintDevicePolicies1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.LintDevicePoliciesResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Effective = (request.DevicePolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Outcome = new ApiDevices.LintDevicePoliciesResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "LintDevicePolicies", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a device policy.
    /// </summary>
    public override async Task<ApiDevices.NormalizeDevicePolicyResponse> NormalizeDevicePolicy(ApiDevices.NormalizeDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        var named0 = _registry.Get<ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient>("acme.devices.v1.DevicePolicyCalculationService");
        using var rpc0 = named0.RecalculateDevicePoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.NormalizeDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Parameters = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "NormalizeDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a device policy.
    /// </summary>
    public override async Task<ApiDevices.PreviewDevicePolicyResponse> PreviewDevicePolicy(ApiDevices.PreviewDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.PreviewSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _licensingSoftwareLicenseValidationService.PreviewSoftwareLicenseAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLicensing.QuoteEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.DevicePolicy?.Owner?.DisplayName ?? ""),
                    Email = (request.DevicePolicy?.Owner?.Email ?? ""),
                },
                Route = new ApiLicensing.EntitlementRouteAction
                {
                    Target = (request.DevicePolicy?.Route?.Target ?? ""),
                },
            },
        };
        using var rpc1 = _licensingEntitlementCalculationService.QuoteEntitlementAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiDevices.PreviewDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Effective = (request.DevicePolicy?.Effective ?? new ApiCommon.TimeWindow()),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
            Outcome = new ApiDevices.PreviewDevicePolicyResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "PreviewDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a device policy.
    /// </summary>
    public override async Task<ApiDevices.ScoreDevicePolicyResponse> ScoreDevicePolicy(ApiDevices.ScoreDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SimulateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SimulateDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.DevicePolicy?.Audit?.CreatedBy ?? ""),
                },
                Discount = new ApiDevices.DevicePolicyDiscountAction
                {
                    Amount = (request.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Discount => (request.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyCalculationService.SimulateDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.DevicePolicy?.Owner?.Email ?? ""),
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
        using var rpc1 = _licensingSoftwareLicenseValidationService.ValidateSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.StreamDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.StreamDocumentFolderExportRequest.Types.Filter
            {
                Labels = { (reply1.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream2 = _documentsDocumentFolderExportService.StreamDocumentFolderExport(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received StreamDocumentFolderExport frame {Type}", change2.GetType().Name);
        }
        var response = new ApiDevices.ScoreDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply1.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                    Etag = (reply1.SoftwareLicense?.Audit?.Etag ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Version = (request.DevicePolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ScoreDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a device policy.
    /// </summary>
    public override async Task<ApiDevices.ValidateDevicePolicyResponse> ValidateDevicePolicy(ApiDevices.ValidateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.NormalizeEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendValidateDevicePolicy0Async(input0, token);
        var response = new ApiDevices.ValidateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Description = (reply0.Entitlement?.Description ?? ""),
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Effective = (request.DevicePolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ValidateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.NormalizeEntitlementResponse> SendValidateDevicePolicy0Async(ApiLicensing.NormalizeEntitlementRequest request, CancellationToken token)
    {
        return await _devicePolicyValidationEndpointSendValidateDevicePolicy0Port.SendAsync(request, token);
    }
}
