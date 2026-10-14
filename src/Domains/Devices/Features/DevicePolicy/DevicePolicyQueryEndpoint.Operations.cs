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
using ApiLicensing = Acme.Licensing.V1;
using ApiRatelimiting = Acme.Ratelimiting.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Devices;

public sealed partial class DevicePolicyQueryEndpoint
{
    /// <summary>
    /// Lists device policies a page at a time.
    /// </summary>
    public override async Task<ApiDevices.BrowseDevicePoliciesResponse> BrowseDevicePolicies(ApiDevices.BrowseDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "BrowseDevicePolicies", "accepted", token);
        var input0 = new ApiDevices.EstimateDevicePolicyRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.EstimateDevicePolicyRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiDevices.DevicePolicyCalculationService.DevicePolicyCalculationServiceClient>("acme.devices.v1.DevicePolicyCalculationService");
        using var rpc0 = named0.EstimateDevicePolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.CheckComplianceScanEligibilityRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                    Email = (reply0.DevicePolicy?.Owner?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CheckComplianceScanEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
                    {
                        ApiDevices.DevicePolicy.ActionOneofCase.Block => (reply0.DevicePolicy?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _devicesComplianceScanValidationService.CheckComplianceScanEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.BrowseDevicePoliciesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "BrowseDevicePolicies", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts device policies by status.
    /// </summary>
    public override async Task<ApiDevices.CountDevicePoliciesResponse> CountDevicePolicies(ApiDevices.CountDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "CountDevicePolicies", "accepted", token);
        var input0 = new ApiDevices.ValidateComplianceScanRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ValidateComplianceScanRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _devicesComplianceScanValidationService.ValidateComplianceScanAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiDevices.CountDevicePoliciesRequest, ApiRatelimiting.SimulateRateLimitRequest>(value => new ApiRatelimiting.SimulateRateLimitRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRatelimiting.SimulateRateLimitRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _ratelimitingRateLimitCalculationService.SimulateRateLimitAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiDevices.CountDevicePoliciesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "CountDevicePolicies", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching device policy.
    /// </summary>
    public override async Task ExportDevicePolicies(ApiDevices.ExportDevicePoliciesRequest request, IServerStreamWriter<ApiDevices.ExportDevicePoliciesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "ExportDevicePolicies", "accepted", token);
        var input0 = new ApiDevices.SuggestManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.SuggestManagedDevicesRequest.Types.Filter
            {
                Region = (request.Filter?.Region ?? ""),
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _devicesManagedDeviceQueryService.SuggestManagedDevicesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.ExportDevicePoliciesToCheckDocumentEligibility1();
        using var rpc1 = _documentsDocumentValidationService.CheckDocumentEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiVenues.NormalizeVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.Document?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.Document?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply1.Document?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        var reply2 = await SendExportDevicePolicies2Async(input2, token);
        var response = new ApiDevices.ExportDevicePoliciesResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Document?.Audit?.CreatedBy ?? ""),
                },
                Name = (reply2.Venue?.Name ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Outcome?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "ExportDevicePolicies", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists device policies a page at a time.
    /// </summary>
    public override async Task<ApiDevices.FindDevicePoliciesResponse> FindDevicePolicies(ApiDevices.FindDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "FindDevicePolicies", "accepted", token);
        var input0 = new ApiLicensing.BrowseSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.BrowseSoftwareLicensesRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
            },
            Page = new ApiCommon.PageRequest
            {
                PageToken = (request.Page?.PageToken ?? ""),
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _licensingSoftwareLicenseQueryService.BrowseSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.FindDevicePoliciesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OrderBy ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "FindDevicePolicies", (request.OrderBy ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.LookupDevicePolicyResponse> LookupDevicePolicy(ApiDevices.LookupDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        _logger.LogInformation("Handling LookupDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "LookupDevicePolicy", "accepted", token);
        var input0 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ValidateDocumentRequest>();
        using var rpc0 = _documentsDocumentValidationService.ValidateDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.LookupDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a device policy.
    /// </summary>
    public override async Task<ApiDevices.PeekDevicePolicyResponse> PeekDevicePolicy(ApiDevices.PeekDevicePolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.DevicePolicyId, "device_policy_id");
        _logger.LogInformation("Handling PeekDevicePolicy for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "PeekDevicePolicy", "accepted", token);
        var response = new ApiDevices.PeekDevicePolicyResponse
        {
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                DevicePolicyId = (request.DevicePolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several device policies at once.
    /// </summary>
    public override async Task<ApiDevices.ResolveDevicePoliciesResponse> ResolveDevicePolicies(ApiDevices.ResolveDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "ResolveDevicePolicies", "accepted", token);
        var response = new ApiDevices.ResolveDevicePoliciesResponse();
        return response;
    }
    /// <summary>
    /// Lists device policies a page at a time.
    /// </summary>
    public override async Task<ApiDevices.SuggestDevicePoliciesResponse> SuggestDevicePolicies(ApiDevices.SuggestDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", "batch",
            "SuggestDevicePolicies", "accepted", token);
        var input0 = new ApiSeating.SummarizeSeatMapsRequest
        {
            Annotations = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiSeating.SummarizeSeatMapsRequest>(json0);
        using var rpc0 = _seatingSeatMapQueryService.SummarizeSeatMapsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiDevices.SuggestDevicePoliciesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "SuggestDevicePolicies", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a device policy.
    /// </summary>
    public override async Task<ApiDevices.SummarizeDevicePoliciesResponse> SummarizeDevicePolicies(ApiDevices.SummarizeDevicePoliciesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeDevicePolicies for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "device_policy", request.DevicePolicyId,
            "SummarizeDevicePolicies", "accepted", token);
        var input0 = new ApiSearch.NormalizeSearchIndexRequest();
        input0.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Reason = (request.Reason ?? "");
        input0.SearchIndex = new ApiSearch.SearchIndex
        {
            Owner = new ApiIdentity.UserRef
            {
                Email = (request.Context?.Actor?.Email ?? ""),
            },
        };
        using var rpc0 = _searchSearchIndexValidationService.NormalizeSearchIndexAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.LintComplianceScansRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.SearchIndex?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.LintComplianceScansRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.SearchIndex?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanValidationService.LintComplianceScansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiDevices.LintComplianceScansRequest>("acme.devices.v1.DevicePolicyQueryService/SummarizeDevicePolicies/1", input1.Clone()), token);
        var response = new ApiDevices.SummarizeDevicePoliciesResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.DevicePolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "device_policy",
            "audit", "SummarizeDevicePolicies", (request.DevicePolicyId ?? ""), token);
        return response;
    }
    private async Task<ApiVenues.NormalizeVenueResponse> SendExportDevicePolicies2Async(ApiVenues.NormalizeVenueRequest request, CancellationToken token)
    {
        return await _devicePolicyQueryEndpointSendExportDevicePolicies2Port.SendAsync(request, token);
    }
}
