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
using ApiCustomers = Acme.Customers.V1;
using ApiDevices = Acme.Devices.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInvoicing = Acme.Invoicing.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Devices;

public sealed partial class DevicePolicyApprovalEndpoint
{
    /// <summary>
    /// Approves a device policy.
    /// </summary>
    public override async Task<ApiDevices.ApproveDevicePolicyResponse> ApproveDevicePolicy(ApiDevices.ApproveDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CreateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.DevicePolicy?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _devicesManagedDeviceService.CreateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.BrowseJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseJobOpeningsRequest.Types.Filter
            {
                Region = (reply0.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (reply0.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningQueryService.BrowseJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ApproveDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.ManagedDevice?.Audit?.Etag ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = (request.DevicePolicy?.Owner ?? new ApiIdentity.UserRef()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ApproveDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a device policy.
    /// </summary>
    public override async Task<ApiDevices.DelegateDevicePolicyApprovalResponse> DelegateDevicePolicyApproval(ApiDevices.DelegateDevicePolicyApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateDevicePolicyApproval for {Tenant}", requestContext.TenantId);
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
                NewOwner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            DevicePolicyId = (request.DevicePolicy?.DevicePolicyId ?? ""),
        };
        var reply0 = await SendDelegateDevicePolicyApproval0Async(input0, token);
        var input1 = new ApiDevices.PreviewComplianceScanRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.PreviewComplianceScanRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanValidationService.PreviewComplianceScanAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.DelegateDevicePolicyApprovalResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Parameters = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Outcome = new ApiDevices.DelegateDevicePolicyApprovalResponse.Types.Outcome
            {
                EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "DelegateDevicePolicyApproval", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.GetDevicePolicyApprovalStatusResponse> GetDevicePolicyApprovalStatus(ApiDevices.GetDevicePolicyApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        _logger.LogInformation("Handling GetDevicePolicyApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "GetDevicePolicyApprovalStatus", "accepted", token);
        var input0 = new ApiAuthorization.FindAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.FindAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.DevicePolicyId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiAuthorization.FindAccessPoliciesRequest>();
        using var rpc0 = _authorizationAccessPolicyQueryService.FindAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.GetDevicePolicyApprovalStatusResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        return response;
    }
    /// <summary>
    /// Lists device policies a page at a time.
    /// </summary>
    public override async Task<ApiDevices.ListPendingDevicePoliciesResponse> ListPendingDevicePolicies(ApiDevices.ListPendingDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "ListPendingDevicePolicies", "accepted", token);
        var input0 = new ApiDevices.NormalizeComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                OccurredAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _devicesComplianceScanValidationService.NormalizeComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ListPendingDevicePoliciesResponse
        {
            DevicePolicies = { new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ComplianceScan?.Subject?.DisplayName ?? ""),
                    Email = (reply0.ComplianceScan?.Subject?.Email ?? ""),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ListPendingDevicePolicies", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a device policy.
    /// </summary>
    public override async Task<ApiDevices.RecallDevicePolicyResponse> RecallDevicePolicy(ApiDevices.RecallDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.RecallSoftwareLicenseRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Details = new ApiLicensing.RecallSoftwareLicenseRequest.Types.Details
        {
            Approver = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.Approver?.DisplayName ?? ""),
            },
        };
        input0.SoftwareLicense = new ApiLicensing.SoftwareLicense
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (request.DevicePolicy?.Audit?.CreatedAt ?? new Timestamp()),
                UpdatedBy = (request.DevicePolicy?.Audit?.UpdatedBy ?? ""),
            },
        };
        using var rpc0 = _licensingSoftwareLicenseApprovalService.RecallSoftwareLicenseAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.CreateManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.SoftwareLicense?.Holder?.DisplayName ?? ""),
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                    UserId = (request.Details?.Approver?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _devicesManagedDeviceService.CreateManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.RecallDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.SoftwareLicense?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                },
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
                Parameters = { (request.DevicePolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "RecallDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a device policy.
    /// </summary>
    public override async Task<ApiDevices.RejectDevicePolicyResponse> RejectDevicePolicy(ApiDevices.RejectDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.CancelEntitlementWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.DevicePolicy?.Audit?.CreatedBy ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementWorkflowService.CancelEntitlementWorkflowAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiDevices.RejectDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Effective = (request.DevicePolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "RejectDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a device policy.
    /// </summary>
    public override async Task<ApiDevices.SubmitDevicePolicyResponse> SubmitDevicePolicy(ApiDevices.SubmitDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitDevicePolicy for {Tenant}", requestContext.TenantId);
        if (request.DevicePolicy is { } record)
        {
            var snapshot = DevicePolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiItsm.CalculateChangeRequestRequest
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _itsmChangeRequestCalculationService.CalculateChangeRequestAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiInvoicing.UpdateInvoiceBatchRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            InvoiceBatch = new ApiInvoicing.InvoiceBatch
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.ChangeRequest?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.DevicePolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiCustomers.CustomerRef
                {
                    Email = (reply0.ChangeRequest?.Assignee?.Email ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _invoicingInvoiceBatchService.UpdateInvoiceBatchAsync);
        var response = new ApiDevices.SubmitDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.ChangeRequest?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Description = (request.DevicePolicy?.Description ?? ""),
                DevicePolicyId = (request.DevicePolicyId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.InvoiceBatch?.Counterparty?.DisplayName ?? ""),
                },
            },
            Outcome = new ApiDevices.SubmitDevicePolicyResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "SubmitDevicePolicy", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.SuspendDevicePolicyResponse> SendDelegateDevicePolicyApproval0Async(ApiDevices.SuspendDevicePolicyRequest request, CancellationToken token)
    {
        return await _devicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port.SendAsync(request, token);
    }
}
