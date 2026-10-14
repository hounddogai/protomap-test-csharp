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
using ApiAnomalies = Acme.Anomalies.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Devices;

public sealed partial class ManagedDeviceCalculationEndpoint
{
    /// <summary>
    /// Calculates a managed device.
    /// </summary>
    public override async Task<ApiDevices.CalculateManagedDeviceResponse> CalculateManagedDevice(ApiDevices.CalculateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAnomalies.CreateAnomalyRequest();
        input0.Anomaly = new ApiAnomalies.Anomaly
        {
            Audit = new ApiCommon.AuditInfo
            {
                Etag = (request.ManagedDevice?.Audit?.Etag ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _anomaliesAnomalyService.CreateAnomalyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Anomaly?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Anomaly?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Anomaly?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (reply0.Anomaly ?? new ApiAnomalies.Anomaly()).OriginCase switch
                    {
                        ApiAnomalies.Anomaly.OriginOneofCase.Customer => (reply0.Anomaly?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _licensingSoftwareLicenseValidationService.ValidateSoftwareLicenseAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.RecalculateSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (reply1.SoftwareLicense?.Contact?.Phone ?? ""),
                },
            },
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _licensingSoftwareLicenseCalculationService.RecalculateSoftwareLicensesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiLicensing.CalculateLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Anomaly?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Card = new ApiLicensing.LicenseAssignmentCardSettlement
                {
                    Last4 = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.Last4 ?? ""),
                        _ => ""
                    },
                    Token = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    Version = (reply0.Anomaly ?? new ApiAnomalies.Anomaly()).OriginCase switch
                    {
                        ApiAnomalies.Anomaly.OriginOneofCase.Customer => (reply0.Anomaly?.Customer?.Version ?? 0L),
                        _ => 0L
                    },
                },
            },
        };
        using var rpc3 = _licensingLicenseAssignmentCalculationService.CalculateLicenseAssignmentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiDevices.CalculateManagedDeviceResponse
        {
            Breakdown = new ApiDevices.CalculateManagedDeviceResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
                Total = (reply2.Breakdown?.Total ?? new ApiCommon.Money()),
            },
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply3.LicenseAssignment?.Audit?.Etag ?? ""),
                },
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.SoftwareLicense?.Contact?.Email ?? ""),
                    UserId = (reply0.Anomaly?.Assignee?.UserId ?? ""),
                },
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "CalculateManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a managed device.
    /// </summary>
    public override async Task<ApiDevices.EstimateManagedDeviceResponse> EstimateManagedDevice(ApiDevices.EstimateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.ManagedDevice?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
                Holder = new ApiLicensing.LicenseAssignmentRef
                {
                    DisplayName = (request.ManagedDevice?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _licensingSoftwareLicenseValidationService.ValidateSoftwareLicenseAsync);
        var input1 = new ApiPatients.ClonePatientRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Patient = new ApiPatients.Patient
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.ManagedDevice?.Owner?.UserId ?? ""),
                },
            },
        };
        using var rpc1 = _sendEstimateManagedDevice1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.EstimateManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Patient?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "EstimateManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a managed device.
    /// </summary>
    public override async Task<ApiDevices.ExplainManagedDeviceCalculationResponse> ExplainManagedDeviceCalculation(ApiDevices.ExplainManagedDeviceCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainManagedDeviceCalculation for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.LintDocumentFoldersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
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
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.ManagedDevice?.Owner?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderValidationService.LintDocumentFoldersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ResolveDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor1 = new Method<ApiDocuments.ResolveDocumentsRequest, ApiDocuments.ResolveDocumentsResponse>(
            MethodType.Unary, "acme.documents.v1" + ".DocumentQueryService", "ResolveDocuments",
            Wire.Marshaller<ApiDocuments.ResolveDocumentsRequest>(), Wire.Marshaller<ApiDocuments.ResolveDocumentsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.ExplainManagedDeviceCalculationResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Capacity = (request.ManagedDevice?.Capacity ?? 0L),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.DocumentFolder?.Manager?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "ExplainManagedDeviceCalculation", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Quotes a managed device.
    /// </summary>
    public override async Task<ApiDevices.QuoteManagedDeviceResponse> QuoteManagedDevice(ApiDevices.QuoteManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.LintUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.ManagedDevice?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiIdentity.LintUsersRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _identityUserValidationService.LintUsersAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.QuoteManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Code = (request.ManagedDevice?.Code ?? ""),
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.User?.RelationshipManager?.UserId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "QuoteManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a managed device.
    /// </summary>
    public override async Task<ApiDevices.RecalculateManagedDevicesResponse> RecalculateManagedDevices(ApiDevices.RecalculateManagedDevicesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling RecalculateManagedDevices for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiDevices.RecalculateManagedDevicesRequest, ApiLicensing.EstimateEntitlementRequest>(value => new ApiLicensing.EstimateEntitlementRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EstimateEntitlementRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (value.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _licensingEntitlementCalculationService.EstimateEntitlementAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.NormalizeLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.NormalizeLicenseAssignmentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Entitlement?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                    {
                        ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Quantity = (request.ManagedDevice?.Components?.GetValueOrDefault("primary")?.Quantity ?? 0L),
                } },
            },
        };
        using var rpc1 = _licensingLicenseAssignmentValidationService.NormalizeLicenseAssignmentAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiDocuments.NormalizeDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                            UpdatedBy = (request.ManagedDevice?.Audit?.UpdatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                            {
                                ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Locality ?? ""),
                                _ => ""
                            },
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.ManagedDevice?.Owner?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc2 = _documentsDocumentFolderValidationService.NormalizeDocumentFolderAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiDevices.RecalculateManagedDevicesResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply2.DocumentFolder?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Status = (request.ManagedDevice?.Status ?? (ApiDevices.ManagedDeviceStatus)0),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "RecalculateManagedDevices", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a managed device.
    /// </summary>
    public override async Task<ApiDevices.SimulateManagedDeviceResponse> SimulateManagedDevice(ApiDevices.SimulateManagedDeviceRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateManagedDevice for {Tenant}", requestContext.TenantId);
        if (request.ManagedDevice is { } record)
        {
            var snapshot = ManagedDeviceMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ValidateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.ManagedDevice?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.ManagedDevice?.Owner?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendSimulateManagedDevice0Async(input0, token);
        var input1 = new ApiMetrics.BrowseMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (reply0.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.ManagedDevice ?? new ApiDevices.ManagedDevice()).LocatorCase switch
                {
                    ApiDevices.ManagedDevice.LocatorOneofCase.Address => (request.ManagedDevice?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _metricsMetricSeriesQueryService.BrowseMetricSeriesesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.SimulateManagedDeviceResponse
        {
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                ManagedDeviceId = (request.ManagedDeviceId ?? ""),
                Name = (request.ManagedDevice?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply0.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "managed_device",
            "audit", "SimulateManagedDevice", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.ValidateSoftwareLicenseResponse> SendSimulateManagedDevice0Async(ApiLicensing.ValidateSoftwareLicenseRequest request, CancellationToken token)
    {
        return await _managedDeviceCalculationEndpointSendSimulateManagedDevice0Port.SendAsync(request, token);
    }
}
