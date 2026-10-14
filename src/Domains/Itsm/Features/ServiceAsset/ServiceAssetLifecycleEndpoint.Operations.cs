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
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ServiceAssetLifecycleEndpoint
{
    /// <summary>
    /// Activates a service asset.
    /// </summary>
    public override async Task<ApiItsm.ActivateServiceAssetResponse> ActivateServiceAsset(ApiItsm.ActivateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling ActivateServiceAsset for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "ActivateServiceAsset", "accepted", token);
        var input0 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _employeesOrgUnitQueryService.SummarizeOrgUnitsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ActivateServiceAssetResponse
        {
            Outcome = new ApiItsm.ActivateServiceAssetResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ActivateServiceAsset", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires a service asset.
    /// </summary>
    public override async Task<ApiItsm.ExpireServiceAssetResponse> ExpireServiceAsset(ApiItsm.ExpireServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling ExpireServiceAsset for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "ExpireServiceAsset", "accepted", token);
        var input0 = new ApiEmployees.CloneEmployeeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.CloneEmployeeRequest>();
        using var rpc0 = _employeesEmployeeService.CloneEmployeeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.SyncProblemsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.SyncProblemsRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Problem = new ApiItsm.Problem
            {
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _itsmProblemSyncService.SyncProblemsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.QuoteServiceAssetRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Details = new ApiItsm.QuoteServiceAssetRequest.Types.Details
        {
            Destination = new ApiCommon.PostalAddress
            {
                Recipient = (reply0.Employee?.PrimaryAddress?.Recipient ?? ""),
            },
        };
        input2.ServiceAsset = new ApiItsm.ServiceAsset
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedBy = (reply1.Problem?.Audit?.CreatedBy ?? ""),
            },
        };
        using var rpc2 = _itsmServiceAssetCalculationService.QuoteServiceAssetAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.GetServiceAssetRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAssetId = (request.ServiceAssetId ?? ""),
        };
        using var rpc3 = _itsmServiceAssetCalculationService.GetServiceAssetRatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ExpireServiceAssetResponse
        {
            Outcome = new ApiItsm.ExpireServiceAssetResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Result?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Dimensions = new ApiItsm.ServiceAssetDimensions
                {
                    Weight = (reply2.ServiceAsset?.Dimensions?.Weight ?? 0d),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply1.Problem?.Customer?.Email ?? ""),
                },
                ServiceAssetId = (reply3.ServiceAsset?.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ExpireServiceAsset", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a service asset.
    /// </summary>
    public override async Task<ApiItsm.GetServiceAssetLifecycleResponse> GetServiceAssetLifecycle(ApiItsm.GetServiceAssetLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling GetServiceAssetLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "GetServiceAssetLifecycle", "accepted", token);
        var input0 = new ApiItsm.ListChangeRequestReportsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListChangeRequestReportsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                Labels = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ServiceAssetId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _itsmChangeRequestReportingService.ListChangeRequestReportsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeOrgUnitsRequest.Types.Details
            {
                Filters = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesOrgUnitQueryService.SummarizeOrgUnitsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.GetServiceAssetLifecycleResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Reinstates a service asset.
    /// </summary>
    public override async Task<ApiItsm.ReinstateServiceAssetResponse> ReinstateServiceAsset(ApiItsm.ReinstateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling ReinstateServiceAsset for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "ReinstateServiceAsset", "accepted", token);
        var input0 = new ApiEmployees.FindEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.FindEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesEmployeeQueryService.FindEmployeesAsync);
        var response = new ApiItsm.ReinstateServiceAssetResponse
        {
            Outcome = new ApiItsm.ReinstateServiceAssetResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ReinstateServiceAsset", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Renews a service asset.
    /// </summary>
    public override async Task<ApiItsm.RenewServiceAssetResponse> RenewServiceAsset(ApiItsm.RenewServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling RenewServiceAsset for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "RenewServiceAsset", "accepted", token);
        var input0 = new ApiNotifications.CloneNotificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Notification = new ApiNotifications.Notification
            {
                Customer = new ApiNotifications.NotificationTemplateRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        using var rpc0 = _sendRenewServiceAsset0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.DeleteServiceAssetReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Etag = (reply0.Notification?.Audit?.Etag ?? ""),
            Reason = (request.Reason ?? ""),
            ServiceAssetId = (request.ServiceAssetId ?? ""),
        };
        var named1 = _registry.Get<ApiItsm.ServiceAssetReportingService.ServiceAssetReportingServiceClient>("acme.itsm.v1.ServiceAssetReportingService");
        using var rpc1 = named1.DeleteServiceAssetReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                Labels = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Notification?.Customer?.NotificationTemplateId ?? ""),
            },
        };
        using var stream2 = _employeesOrgUnitQueryService.ExportOrgUnits(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change2.GetType().Name);
        }
        var response = new ApiItsm.RenewServiceAssetResponse
        {
            Outcome = new ApiItsm.RenewServiceAssetResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "RenewServiceAsset", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a service asset.
    /// </summary>
    public override async Task<ApiItsm.ScheduleServiceAssetExpiryResponse> ScheduleServiceAssetExpiry(ApiItsm.ScheduleServiceAssetExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleServiceAssetExpiry for {Tenant}", requestContext.TenantId);
        if (request.ServiceAsset is { } record)
        {
            var snapshot = ServiceAssetMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ListDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentsRequest.Types.Filter
            {
                CreatedAfter = (request.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.ServiceAsset?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.ServiceAsset?.ServiceAssetId ?? ""),
                Region = (request.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                {
                    ApiItsm.ServiceAsset.LocatorOneofCase.Address => (request.ServiceAsset?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc0 = _documentsDocumentService.ListDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.ScheduleServiceAssetExpiryRequest, ApiItsm.PreviewServiceAssetRequest>(value => new ApiItsm.PreviewServiceAssetRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PreviewServiceAssetRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (value.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (value.Details?.Notify?.PreferredChannel ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (value.Details?.Notify?.Email ?? ""),
                    UserId = (value.ServiceAsset?.Owner?.UserId ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _itsmServiceAssetValidationService.PreviewServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ScheduleServiceAssetExpiryResponse
        {
            Outcome = new ApiItsm.ScheduleServiceAssetExpiryResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Name = (request.ServiceAsset?.Name ?? ""),
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAsset?.Owner?.Email ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ScheduleServiceAssetExpiry", (request.ServiceAsset?.Owner?.Email ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends a service asset.
    /// </summary>
    public override async Task<ApiItsm.SuspendServiceAssetResponse> SuspendServiceAsset(ApiItsm.SuspendServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling SuspendServiceAsset for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "SuspendServiceAsset", "accepted", token);
        var input0 = new ApiItsm.CheckServiceAssetEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.CheckServiceAssetEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _itsmServiceAssetValidationService.CheckServiceAssetEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SuspendServiceAssetToGenerateChangeRequestReport1(reply0);
        using var rpc1 = _itsmChangeRequestReportingService.GenerateChangeRequestReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.SuspendServiceAssetResponse
        {
            Outcome = new ApiItsm.SuspendServiceAssetResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "SuspendServiceAsset", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates a service asset.
    /// </summary>
    public override async Task<ApiItsm.TerminateServiceAssetResponse> TerminateServiceAsset(ApiItsm.TerminateServiceAssetRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling TerminateServiceAsset for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "TerminateServiceAsset", "accepted", token);
        var input0 = new ApiDevices.StartDevicePolicyWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.StartDevicePolicyWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            DevicePolicy = new ApiDevices.DevicePolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
        };
        var reply0 = await SendTerminateServiceAsset0Async(input0, token);
        var input1 = new ApiItsm.ExplainServiceAssetDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainServiceAssetDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply0.DevicePolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _itsmServiceAssetValidationService.ExplainServiceAssetDecisionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiVenues.CheckVenueEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiVenues.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply1.ServiceAsset?.Address?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()),
                        _ => new Google.Protobuf.Collections.RepeatedField<string>()
                    } },
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ServiceAsset?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.DevicePolicy?.Audit?.UpdatedBy ?? ""),
                },
                Dimensions = new ApiVenues.VenueDimensions
                {
                    Unit = (reply1.ServiceAsset?.Dimensions?.Unit ?? ""),
                },
            },
        };
        using var rpc2 = _venuesVenueValidationService.CheckVenueEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiEmployees.SummarizeEmployeesRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.SummarizeEmployeesRequest.Types.Details
            {
                Filters = { (reply2.Venue?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Period = (reply0.DevicePolicy?.Effective ?? new ApiCommon.TimeWindow()),
            },
            Reason = (reply0.DevicePolicy ?? new ApiDevices.DevicePolicy()).ActionCase switch
            {
                ApiDevices.DevicePolicy.ActionOneofCase.Block => (reply0.DevicePolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc3 = _employeesEmployeeQueryService.SummarizeEmployeesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.TerminateServiceAssetResponse
        {
            Outcome = new ApiItsm.TerminateServiceAssetResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Components = { { "primary", new ApiItsm.ServiceAssetComponent
                {
                    Description = (reply0.DevicePolicy?.Description ?? ""),
                } } },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "TerminateServiceAsset", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiDevices.StartDevicePolicyWorkflowResponse> SendTerminateServiceAsset0Async(ApiDevices.StartDevicePolicyWorkflowRequest request, CancellationToken token)
    {
        return await _serviceAssetLifecycleEndpointSendTerminateServiceAsset0Port.SendAsync(request, token);
    }
}
