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
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiNotifications = Acme.Notifications.V1;

namespace Acme.Workforce.Devices;

public sealed partial class DevicePolicyCalculationEndpoint
{
    /// <summary>
    /// Calculates a device policy.
    /// </summary>
    public override async Task<ApiDevices.CalculateDevicePolicyResponse> CalculateDevicePolicy(ApiDevices.CalculateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiNotifications.BatchGetNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _sendCalculateDevicePolicy0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.CalculateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Name = (request.DevicePolicy?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "CalculateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a device policy.
    /// </summary>
    public override async Task<ApiDevices.CompareDevicePolicyOptionsResponse> CompareDevicePolicyOptions(ApiDevices.CompareDevicePolicyOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareDevicePolicyOptions for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SimulateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.DevicePolicy?.Owner?.Email ?? ""),
                    UserId = (request.DevicePolicy?.Owner?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiDevices.ManagedDeviceCalculationService.ManagedDeviceCalculationServiceClient>("acme.devices.v1.ManagedDeviceCalculationService");
        using var rpc0 = named0.SimulateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDevices.SimulateManagedDeviceRequest>("acme.devices.v1.DevicePolicyCalculationService/CompareDevicePolicyOptions/0", input0.Clone()), token);
        var input1 = new ApiDevices.QuoteManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Address = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Components = { { "primary", new ApiDevices.ManagedDeviceComponent
                {
                    Description = (reply0.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } } },
            },
        };
        using var rpc1 = _devicesManagedDeviceCalculationService.QuoteManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.CompareDevicePolicyOptionsResponse
        {
            Breakdown = new ApiDevices.CompareDevicePolicyOptionsResponse.Types.Breakdown
            {
                Total = (reply0.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Name = (request.DevicePolicy?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "CompareDevicePolicyOptions", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a device policy.
    /// </summary>
    public override async Task<ApiDevices.EstimateDevicePolicyResponse> EstimateDevicePolicy(ApiDevices.EstimateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiConcessions.BatchGetConcessionStandsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiConcessions.BatchGetConcessionStandsRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _concessionsConcessionStandService.BatchGetConcessionStandsAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.EstimateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Status = (request.DevicePolicy?.Status ?? (ApiDevices.DevicePolicyStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "EstimateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a device policy.
    /// </summary>
    public override async Task<ApiDevices.ExplainDevicePolicyCalculationResponse> ExplainDevicePolicyCalculation(ApiDevices.ExplainDevicePolicyCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainDevicePolicyCalculation for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiDevices.ExplainDevicePolicyCalculationRequest, ApiDevices.SimulateManagedDeviceRequest>(value => new ApiDevices.SimulateManagedDeviceRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (value.DevicePolicy?.Audit?.CreatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _devicesManagedDeviceCalculationService.SimulateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.EstimateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _devicesManagedDeviceCalculationService.EstimateManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.ExplainDevicePolicyCalculationToExplainManagedDeviceCalculation2(reply1);
        using var rpc2 = _devicesManagedDeviceCalculationService.ExplainManagedDeviceCalculationAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDocuments.ListDocumentTemplateConflictsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentTemplateConflictsRequest.Types.Filter
            {
                CreatedAfter = (request.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (request.DevicePolicy?.Owner?.UserId ?? ""),
                Region = (reply2.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply2.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc3 = _documentsDocumentTemplateSyncService.ListDocumentTemplateConflictsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.ExplainDevicePolicyCalculationResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply1.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Name = (request.DevicePolicy?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
            Outcome = new ApiDevices.ExplainDevicePolicyCalculationResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ExplainDevicePolicyCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.GetDevicePolicyRatesResponse> GetDevicePolicyRates(ApiDevices.GetDevicePolicyRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        _logger.LogInformation("Handling GetDevicePolicyRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "GetDevicePolicyRates", "accepted", token);
        var input0 = new ApiLicensing.SummarizeSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SummarizeSoftwareLicensesRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingSoftwareLicenseQueryService.SummarizeSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.GetDevicePolicyRatesResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a device policy.
    /// </summary>
    public override async Task<ApiDevices.QuoteDevicePolicyResponse> QuoteDevicePolicy(ApiDevices.QuoteDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CalculateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (request.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDevices.CalculateManagedDeviceRequest>();
        using var rpc0 = _devicesManagedDeviceCalculationService.CalculateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var json1 = JsonFormatter.Default.Format(input1);
        input1 = JsonParser.Default.Parse<ApiLicensing.ValidateSoftwareLicenseRequest>(json1);
        using var rpc1 = _licensingSoftwareLicenseValidationService.ValidateSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.ResolveDocumentTemplateConflictRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ResolveDocumentTemplateConflictRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentTemplate = new ApiDocuments.DocumentTemplate
            {
                Lines = { new ApiDocuments.DocumentTemplateLine
                {
                    Tax = new ApiDocuments.DocumentTemplateLineTax
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
        using var rpc2 = _documentsDocumentTemplateSyncService.ResolveDocumentTemplateConflictAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.QuoteDevicePolicyResponse
        {
            Breakdown = new ApiDevices.QuoteDevicePolicyResponse.Types.Breakdown
            {
                CurrencyCode = (reply0.Breakdown?.CurrencyCode ?? ""),
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.SoftwareLicense?.Audit?.CreatedBy ?? ""),
                },
                Description = (request.DevicePolicy?.Description ?? ""),
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "QuoteDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a device policy.
    /// </summary>
    public override async Task<ApiDevices.RecalculateDevicePoliciesResponse> RecalculateDevicePolicies(ApiDevices.RecalculateDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateDevicePolicies for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.ListAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.ListAccessPoliciesRequest.Types.Filter
            {
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc0 = _authorizationAccessPolicyService.ListAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiNotifications.SearchNotificationsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiNotifications.SearchNotificationsRequest.Types.Filter
            {
                CreatedAfter = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.DevicePolicy?.Owner?.UserId ?? ""),
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _notificationsNotificationService.SearchNotificationsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.RecalculateDevicePoliciesResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Name = (request.DevicePolicy?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "RecalculateDevicePolicies", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a device policy.
    /// </summary>
    public override async Task<ApiDevices.SimulateDevicePolicyResponse> SimulateDevicePolicy(ApiDevices.SimulateDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.SimulateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.SimulateManagedDeviceRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.DevicePolicy?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _devicesManagedDeviceCalculationService.SimulateManagedDeviceAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.SimulateDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ManagedDevice?.Owner?.UserId ?? ""),
                },
                Status = (request.DevicePolicy?.Status ?? (ApiDevices.DevicePolicyStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "SimulateDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
}
