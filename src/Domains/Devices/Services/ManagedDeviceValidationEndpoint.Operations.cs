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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Devices;

public sealed partial class ManagedDeviceValidationEndpoint
{
    /// <summary>
    /// Checks a managed device.
    /// </summary>
    public override async Task<ApiDevices.CheckManagedDeviceEligibilityResponse> CheckManagedDeviceEligibility(ApiDevices.CheckManagedDeviceEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckManagedDeviceEligibility for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.EstimateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
                Description = (request.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                    UserId = (request.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient>("acme.devices.v1.DevicePolicyCalculationService");
        using var rpc0 = named0.EstimateDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ExplainEntitlementCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ExplainEntitlementCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    Reason = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Block => (reply0.DevicePolicy?.Block?.Reason ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingEntitlementCalculationService.ExplainEntitlementCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.ExplainEntitlementCalculationRequest>("acme.devices.v1.ManagedDeviceValidationService/CheckManagedDeviceEligibility/1", input1.Clone()), token);
        var input2 = new ApiIdentity.GetUserRulesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.ManagedDevice?.Owner?.UserId ?? ""),
        };
        var command2 = new DispatchCommand<ApiIdentity.GetUserRulesRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _identityUserValidationService.GetUserRulesAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiDevices.CheckManagedDeviceEligibilityRequest, ApiLicensing.CalculateSoftwareLicenseRequest>(value => new ApiLicensing.CalculateSoftwareLicenseRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CalculateSoftwareLicenseRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.User?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _licensingSoftwareLicenseCalculationService.CalculateSoftwareLicenseAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.CheckManagedDeviceEligibilityResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Name = (request.ManagedDevice?.Name ?? ""),
            },
            Outcome = new ApiDevices.CheckManagedDeviceEligibilityResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "CheckManagedDeviceEligibility", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a managed device.
    /// </summary>
    public override async Task<ApiDevices.ExplainManagedDeviceDecisionResponse> ExplainManagedDeviceDecision(ApiDevices.ExplainManagedDeviceDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainManagedDeviceDecision for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CompareDevicePolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CompareDevicePolicyOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Lines = { (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyCalculationService.CompareDevicePolicyOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ExplainEntitlementCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.DevicePolicy?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
                Route = new ApiLicensing.EntitlementRouteAction
                {
                    Weight = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Route => (reply0.DevicePolicy?.Route?.Weight ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc1 = _licensingEntitlementCalculationService.ExplainEntitlementCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ExplainManagedDeviceDecisionResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Entitlement?.Audit?.CreatedBy ?? ""),
                    Etag = (reply1.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply0.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Capacity = (request.ManagedDevice?.Capacity ?? 0L),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Name = (reply0.DevicePolicy?.Name ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ExplainManagedDeviceDecision", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Normalizes a managed device.
    /// </summary>
    public override async Task<ApiDevices.NormalizeManagedDeviceResponse> NormalizeManagedDevice(ApiDevices.NormalizeManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.RecalculateDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateDevicePoliciesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ManagedDevice?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyCalculationService.RecalculateDevicePoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ExplainEntitlementDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Route = new ApiLicensing.EntitlementRouteAction
                {
                    Weight = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Route => (reply0.DevicePolicy?.Route?.Weight ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc1 = _licensingEntitlementValidationService.ExplainEntitlementDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.NormalizeManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Specs = { (request.ManagedDevice?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "NormalizeManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a managed device.
    /// </summary>
    public override async Task<ApiDevices.PreviewManagedDeviceResponse> PreviewManagedDevice(ApiDevices.PreviewManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CalculateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _devicesManagedDeviceCalculationService.CalculateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.EstimateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EstimateDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Locality ?? ""),
                        _ => ""
                    },
                    PostalCode = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.PostalCode ?? ""),
                        _ => ""
                    },
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        using var rpc1 = _devicesDevicePolicyCalculationService.EstimateDevicePolicyAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.PreviewManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.DevicePolicy?.Owner?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "PreviewManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a managed device.
    /// </summary>
    public override async Task<ApiDevices.ScoreManagedDeviceResponse> ScoreManagedDevice(ApiDevices.ScoreManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.PreviewLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Description = (request.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentValidationService.PreviewLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ScoreManagedDeviceResponse
        {
            Breakdown = new ApiDevices.ScoreManagedDeviceResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.LicenseAssignment?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ScoreManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a managed device.
    /// </summary>
    public override async Task<ApiDevices.ValidateManagedDeviceResponse> ValidateManagedDevice(ApiDevices.ValidateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ValidateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ValidateLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentValidationService.ValidateLicenseAssignmentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.CompareDevicePolicyOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _devicesDevicePolicyCalculationService.CompareDevicePolicyOptionsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiFeatureflags.ExportFeatureFlagsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiFeatureflags.ExportFeatureFlagsRequest.Types.Filter
            {
                CreatedAfter = (reply1.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ManagedDevice?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (request.ManagedDevice?.Address?.Region ?? ""),
            },
        };
        using var stream2 = _featureflagsFeatureFlagQueryService.ExportFeatureFlags(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportFeatureFlags frame {Type}", change2.GetType().Name);
        }
        var response = new ApiDevices.ValidateManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.LicenseAssignment?.Counterparty?.DisplayName ?? ""),
                    Email = (reply1.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ValidateManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
}
