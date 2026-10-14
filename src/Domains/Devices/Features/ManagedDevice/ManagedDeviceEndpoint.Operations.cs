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

public sealed partial class ManagedDeviceEndpoint
{
    /// <summary>
    /// Returns several managed devices at once.
    /// </summary>
    public override async Task<ApiDevices.BatchGetManagedDevicesResponse> BatchGetManagedDevices(ApiDevices.BatchGetManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BatchGetManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "BatchGetManagedDevices", "accepted", token);
        var input0 = new ApiDevices.PreviewDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PreviewDevicePolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiDevices.DevicePolicyValidationService.DevicePolicyValidationServiceClient>("acme.devices.v1.DevicePolicyValidationService");
        using var rpc0 = named0.PreviewDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLedger.PreviewLedgerAccountRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.DevicePolicy?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                Holder = new ApiLedger.PostingBatchRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                },
                Limits = { new ApiLedger.LedgerAccountLimit
                {
                    Amount = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Discount => (reply0.DevicePolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc1 = _ledgerLedgerAccountValidationService.PreviewLedgerAccountAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.BatchGetManagedDevicesResponse
        {
            ManagedDevices = { new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.LedgerAccount?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Creates a managed device.
    /// </summary>
    public override async Task<ApiDevices.CreateManagedDeviceResponse> CreateManagedDevice(ApiDevices.CreateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CreateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CheckComplianceScanEligibilityRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    Unit = (request.ManagedDevice?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CheckComplianceScanEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc0 = _devicesComplianceScanValidationService.CheckComplianceScanEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CreateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
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
        using var rpc1 = _documentsDocumentService.CreateDocumentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.ExplainComplianceScanDecisionRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ExplainComplianceScanDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
        };
        using var rpc2 = _devicesComplianceScanValidationService.ExplainComplianceScanDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiIdentity.ValidateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.ValidateUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _identityUserValidationService.ValidateUserAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.CreateManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply3.User?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply3.User?.Audit?.UpdatedBy ?? ""),
                },
                Capacity = (request.ManagedDevice?.Capacity ?? 0L),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.ComplianceScan?.Subject?.DisplayName ?? ""),
                },
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "CreateManagedDevice", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a managed device.
    /// </summary>
    public override async Task<ApiDevices.DeleteManagedDeviceResponse> DeleteManagedDevice(ApiDevices.DeleteManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ManagedDeviceId, "managed_device_id");
        _logger.LogInformation("Handling DeleteManagedDevice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", request.ManagedDeviceId,
            "DeleteManagedDevice", "accepted", token);
        var input0 = new ApiDocuments.ValidateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
        };
        using var rpc0 = _documentsSignatureValidationService.ValidateSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.NormalizeSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Entitlement?.Owner?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (reply0.Signature?.OccurredAt ?? new Timestamp()),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.Signature?.Subject?.DisplayName ?? ""),
                    DocumentId = (reply0.Signature?.Subject?.DocumentId ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureValidationService.NormalizeSignatureAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.CompareLicenseAssignmentOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Amount = (reply1.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                    {
                        ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply1.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                    Attributes = { (reply0.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        using var rpc3 = _licensingLicenseAssignmentCalculationService.CompareLicenseAssignmentOptionsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.DeleteManagedDeviceResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply3.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ManagedDeviceId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "DeleteManagedDevice", (request.ManagedDeviceId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a managed device.
    /// </summary>
    public override async Task<ApiDevices.GetManagedDeviceResponse> GetManagedDevice(ApiDevices.GetManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ManagedDeviceId, "managed_device_id");
        _logger.LogInformation("Handling GetManagedDevice for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", request.ManagedDeviceId,
            "GetManagedDevice", "accepted", token);
        var input0 = new ApiLicensing.SuggestEntitlementsRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiLicensing.SuggestEntitlementsRequest.Types.Filter
        {
            CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
        };
        using var rpc0 = _licensingEntitlementQueryService.SuggestEntitlementsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.GetManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists managed devices a page at a time.
    /// </summary>
    public override async Task<ApiDevices.ListManagedDevicesResponse> ListManagedDevices(ApiDevices.ListManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "ListManagedDevices", "accepted", token);
        var input0 = new ApiDevices.ExplainManagedDeviceDecisionRequest
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
        using var rpc0 = _devicesManagedDeviceValidationService.ExplainManagedDeviceDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ListManagedDevicesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ListManagedDevices", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes a managed device.
    /// </summary>
    public override async Task<ApiDevices.UpdateManagedDeviceResponse> UpdateManagedDevice(ApiDevices.UpdateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling UpdateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.ValidateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Measurements = { new ApiDevices.ComplianceScanMeasurement
                {
                    Unit = (request.ManagedDevice?.Dimensions?.Unit ?? ""),
                } },
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanValidationService.ValidateComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.BrowseEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.BrowseEntitlementsRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (reply0.Outcome?.ReferenceId ?? ""),
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _licensingEntitlementQueryService.BrowseEntitlementsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.NormalizeSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeSignatureRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
            },
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc2 = _documentsSignatureValidationService.NormalizeSignatureAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        var input3 = new ApiLicensing.EstimateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EstimateEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.ManagedDevice?.Address?.CountryCode ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ManagedDevice?.Audit?.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                    UserId = (request.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc3 = _licensingEntitlementCalculationService.EstimateEntitlementAsync(input3, cancellationToken: token);
        var pending3 = rpc3.ResponseAsync;
        await Task.WhenAll(pending2, pending3);
        var reply2 = await pending2;
        var reply3 = await pending3;
        var response = new ApiDevices.UpdateManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply3.Entitlement?.Audit?.UpdatedBy ?? ""),
                },
                Components = { { "primary", new ApiDevices.ManagedDeviceComponent
                {
                    Description = (reply3.Entitlement?.Description ?? ""),
                } } },
                ManagedDeviceId = (request.ManagedDevice?.ManagedDeviceId ?? ""),
                Name = (request.ManagedDevice?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Signature?.Subject?.DisplayName ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.ComplianceScan?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ManagedDevice?.FacilityCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "UpdateManagedDevice", (request.ManagedDevice?.FacilityCode ?? ""), token);
        return response;
    }
}
