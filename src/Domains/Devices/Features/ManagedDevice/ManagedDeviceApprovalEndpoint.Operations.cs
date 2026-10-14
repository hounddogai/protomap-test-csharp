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
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Devices;

public sealed partial class ManagedDeviceApprovalEndpoint
{
    /// <summary>
    /// Approves a managed device.
    /// </summary>
    public override async Task<ApiDevices.ApproveManagedDeviceResponse> ApproveManagedDevice(ApiDevices.ApproveManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ApproveManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.UpdateManagedDeviceRequest
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
                    Lines = { (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Recipient = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Recipient ?? ""),
                        _ => ""
                    },
                },
                Dimensions = new ApiDevices.ManagedDeviceDimensions
                {
                    Length = (request.ManagedDevice?.Dimensions?.Length ?? 0d),
                },
            },
        };
        using var rpc0 = _devicesManagedDeviceService.UpdateManagedDeviceAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainInterviewDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.ManagedDevice?.Owner?.Email ?? ""),
                    },
                } },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _recruitingInterviewValidationService.ExplainInterviewDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.RecalculateManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.RecalculateManagedDevicesRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Recipient = (reply1.Interview?.Location?.Recipient ?? ""),
                    Region = (reply1.Interview?.Location?.Region ?? ""),
                },
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _devicesManagedDeviceCalculationService.RecalculateManagedDevicesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.ValidateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var json3 = JsonFormatter.Default.Format(input3);
        input3 = JsonParser.Default.Parse<ApiDevices.ValidateDevicePolicyRequest>(json3);
        using var rpc3 = _devicesDevicePolicyValidationService.ValidateDevicePolicyAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.ApproveManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.ManagedDevice?.Audit?.CreatedBy ?? ""),
                },
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply2.ManagedDevice?.Owner?.Email ?? ""),
                    UserId = (reply1.Interview?.Organizer?.UserId ?? ""),
                },
                Status = (reply2.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Outcome = new ApiDevices.ApproveManagedDeviceResponse.Types.Outcome
            {
                EffectiveAt = (reply3.Outcome?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ApproveManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Delegates a managed device.
    /// </summary>
    public override async Task<ApiDevices.DelegateManagedDeviceApprovalResponse> DelegateManagedDeviceApproval(ApiDevices.DelegateManagedDeviceApprovalRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling DelegateManagedDeviceApproval for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.DelegateLicenseAssignmentApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ManagedDevice?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Lines = { (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                    Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentApprovalService.DelegateLicenseAssignmentApprovalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.LintJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    AccountHolder = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (reply0.LicenseAssignment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.CountryCode ?? ""),
                        _ => ""
                    },
                    Locality = (reply0.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.LintJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.NotifyDocumentFolderWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentFolderWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc2 = _documentsDocumentFolderNotificationService.NotifyDocumentFolderWatchersAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.EstimateComplianceScanRequest
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
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EstimateComplianceScanRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.JobOpening?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        using var rpc3 = _devicesComplianceScanCalculationService.EstimateComplianceScanAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.DelegateManagedDeviceApprovalResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Name = (request.ManagedDevice?.Name ?? ""),
            },
            Outcome = new ApiDevices.DelegateManagedDeviceApprovalResponse.Types.Outcome
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply3.ComplianceScan?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ManagedDevice?.Owner?.UserId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "DelegateManagedDeviceApproval", (request.ManagedDevice?.Owner?.UserId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Escalates a managed device.
    /// </summary>
    public override async Task<ApiDevices.EscalateManagedDeviceResponse> EscalateManagedDevice(ApiDevices.EscalateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EscalateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiRecruiting.SearchJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.SearchJobOpeningsRequest.Types.Filter
            {
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _recruitingJobOpeningService.SearchJobOpeningsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.SubmitLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.SubmitLicenseAssignmentRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Approver?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Description = (request.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Description ?? ""),
                } },
            },
        };
        using var rpc1 = _sendEscalateManagedDevice1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.EscalateManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.LicenseAssignment?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.LicenseAssignment?.Audit?.UpdatedBy ?? ""),
                },
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Outcome = new ApiDevices.EscalateManagedDeviceResponse.Types.Outcome
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "EscalateManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a managed device.
    /// </summary>
    public override async Task<ApiDevices.GetManagedDeviceApprovalStatusResponse> GetManagedDeviceApprovalStatus(ApiDevices.GetManagedDeviceApprovalStatusRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ManagedDeviceId, "managed_device_id");
        _logger.LogInformation("Handling GetManagedDeviceApprovalStatus for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", request.ManagedDeviceId,
            "GetManagedDeviceApprovalStatus", "accepted", token);
        var input0 = new ApiDevices.EstimateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Block = new ApiDevices.DevicePolicyBlockAction
                {
                    NotifyEmail = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient>("acme.devices.v1.DevicePolicyCalculationService");
        using var rpc0 = named0.EstimateDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.GetManagedDeviceApprovalStatusResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists managed devices a page at a time.
    /// </summary>
    public override async Task<ApiDevices.ListPendingManagedDevicesResponse> ListPendingManagedDevices(ApiDevices.ListPendingManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListPendingManagedDevices for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "managed_device", "batch",
            "ListPendingManagedDevices", "accepted", token);
        var input0 = new ApiLicensing.RejectSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.RejectSoftwareLicenseRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var descriptor0 = new Method<ApiLicensing.RejectSoftwareLicenseRequest, ApiLicensing.RejectSoftwareLicenseResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseApprovalService", "RejectSoftwareLicense",
            Wire.Marshaller<ApiLicensing.RejectSoftwareLicenseRequest>(), Wire.Marshaller<ApiLicensing.RejectSoftwareLicenseResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.ListPendingManagedDevicesResponse
        {
            ManagedDevices = { new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.SoftwareLicense?.Audit?.Etag ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ListPendingManagedDevices", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalls a managed device.
    /// </summary>
    public override async Task<ApiDevices.RecallManagedDeviceResponse> RecallManagedDevice(ApiDevices.RecallManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecallManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ListPendingLicenseAssignmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.ListPendingLicenseAssignmentsRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _licensingLicenseAssignmentApprovalService.ListPendingLicenseAssignmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiDevices.RecallManagedDeviceRequest, ApiDevices.CreateManagedDeviceRequest>(value => new ApiDevices.CreateManagedDeviceRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Dimensions = new ApiDevices.ManagedDeviceDimensions
                {
                    Length = (value.ManagedDevice?.Dimensions?.Length ?? 0d),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _devicesManagedDeviceService.CreateManagedDeviceAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.RecallManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "RecallManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Rejects a managed device.
    /// </summary>
    public override async Task<ApiDevices.RejectManagedDeviceResponse> RejectManagedDevice(ApiDevices.RejectManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RejectManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.FindDevicePoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.FindDevicePoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _devicesDevicePolicyQueryService.FindDevicePoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.BrowseJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiRecruiting.BrowseJobOpeningsRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ManagedDevice?.Owner?.UserId ?? ""),
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningQueryService.BrowseJobOpeningsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDevices.PeekManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDeviceId = (request.ManagedDevice?.ManagedDeviceId ?? ""),
        };
        var reply2 = await SendRejectManagedDevice2Async(input2, token);
        await _bus.PublishAsync(new TransportNotification<ApiDevices.PeekManagedDeviceRequest>("acme.devices.v1.ManagedDeviceApprovalService/RejectManagedDevice/2", input2.Clone()), token);
        var input3 = new ApiSearch.ValidateSearchIndexRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SearchIndex = new ApiSearch.SearchIndex
            {
                Dimensions = new ApiSearch.SearchIndexDimensions
                {
                    Unit = (request.ManagedDevice?.Dimensions?.Unit ?? ""),
                },
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _searchSearchIndexValidationService.ValidateSearchIndexAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.RejectManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply3.SearchIndex?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = (reply2.ManagedDevice?.Owner ?? new ApiIdentity.UserRef()),
                Status = (reply2.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Outcome = new ApiDevices.RejectManagedDeviceResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "RejectManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Submits a managed device.
    /// </summary>
    public override async Task<ApiDevices.SubmitManagedDeviceResponse> SubmitManagedDevice(ApiDevices.SubmitManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SubmitManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAppointments.BatchGetAppointmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiAppointments.BatchGetAppointmentsRequest>();
        using var rpc0 = _appointmentsAppointmentService.BatchGetAppointmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.FindManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.FindManagedDevicesRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Details?.Approver?.UserId ?? ""),
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _devicesManagedDeviceQueryService.FindManagedDevicesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.SubmitManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
            },
            Outcome = new ApiDevices.SubmitManagedDeviceResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply1.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "SubmitManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.PeekManagedDeviceResponse> SendRejectManagedDevice2Async(ApiDevices.PeekManagedDeviceRequest request, CancellationToken token)
    {
        return await _managedDeviceApprovalEndpointSendRejectManagedDevice2Port.SendAsync(request, token);
    }
}
