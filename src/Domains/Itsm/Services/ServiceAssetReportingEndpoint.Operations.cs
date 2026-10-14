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
using ApiCheckout = Acme.Checkout.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiDrivers = Acme.Drivers.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiShows = Acme.Shows.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ServiceAssetReportingEndpoint
{
    /// <summary>
    /// Counts service assets by status.
    /// </summary>
    public override async Task<ApiItsm.CountServiceAssetTotalsResponse> CountServiceAssetTotals(ApiItsm.CountServiceAssetTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountServiceAssetTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", "batch",
            "CountServiceAssetTotals", "accepted", token);
        var input0 = new ApiEmployees.WatchEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.WatchEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Filter?.Region ?? ""),
            },
        };
        using var stream0 = _employeesEmployeeService.WatchEmployees(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchEmployees frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiIdentity.ScoreUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc1 = _identityUserValidationService.ScoreUserAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.CountServiceAssetTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "CountServiceAssetTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a service asset.
    /// </summary>
    public override async Task<ApiItsm.DeleteServiceAssetReportResponse> DeleteServiceAssetReport(ApiItsm.DeleteServiceAssetReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling DeleteServiceAssetReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "DeleteServiceAssetReport", "accepted", token);
        var input0 = new ApiEmployees.ListEmployeesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ListEmployeesRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.ServiceAssetId ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _employeesEmployeeService.ListEmployeesAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLedger.CheckLedgerAccountEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LedgerAccount = new ApiLedger.LedgerAccount
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _ledgerLedgerAccountValidationService.CheckLedgerAccountEligibilityAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiItsm.DeleteServiceAssetReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a service asset.
    /// </summary>
    public override async Task<ApiItsm.FetchServiceAssetReportResponse> FetchServiceAssetReport(ApiItsm.FetchServiceAssetReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ServiceAssetId, "service_asset_id");
        _logger.LogInformation("Handling FetchServiceAssetReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "FetchServiceAssetReport", "accepted", token);
        var input0 = new ApiCheckout.SearchCheckoutSessionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCheckout.SearchCheckoutSessionsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.ServiceAssetId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _checkoutCheckoutSessionService.SearchCheckoutSessionsAsync);
        var response = new ApiItsm.FetchServiceAssetReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
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
    /// Generates a service asset.
    /// </summary>
    public override async Task<ApiItsm.GenerateServiceAssetReportResponse> GenerateServiceAssetReport(ApiItsm.GenerateServiceAssetReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateServiceAssetReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "GenerateServiceAssetReport", "accepted", token);
        var input0 = new ApiShows.ExportShowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Format = (request.Details?.Format ?? ""),
        };
        using var stream0 = _showsShowQueryService.ExportShows(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportShows frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiItsm.SearchIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.SearchIncidentsRequest.Types.Filter
            {
                Labels = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var named1 = _registry.Get<ApiItsm.IncidentService.IncidentServiceClient>("acme.itsm.v1.IncidentService");
        using var rpc1 = named1.SearchIncidentsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.GenerateServiceAssetReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ServiceAssetId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "GenerateServiceAssetReport", (request.ServiceAssetId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists service assets a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListServiceAssetReportsResponse> ListServiceAssetReports(ApiItsm.ListServiceAssetReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListServiceAssetReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", "batch",
            "ListServiceAssetReports", "accepted", token);
        var input0 = new ApiEmployees.ResolveOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor0 = new Method<ApiEmployees.ResolveOrgUnitsRequest, ApiEmployees.ResolveOrgUnitsResponse>(
            MethodType.Unary, "acme.employees.v1" + ".OrgUnitQueryService", "ResolveOrgUnits",
            Wire.Marshaller<ApiEmployees.ResolveOrgUnitsRequest>(), Wire.Marshaller<ApiEmployees.ResolveOrgUnitsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ListServiceAssetReportsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ListServiceAssetReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a service asset.
    /// </summary>
    public override async Task<ApiItsm.ScheduleServiceAssetReportResponse> ScheduleServiceAssetReport(ApiItsm.ScheduleServiceAssetReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleServiceAssetReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "ScheduleServiceAssetReport", "accepted", token);
        var input0 = new ApiItsm.ExplainServiceAssetCalculationRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        var command0 = new DispatchCommand<ApiItsm.ExplainServiceAssetCalculationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _itsmServiceAssetCalculationService.ExplainServiceAssetCalculationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiItsm.ScheduleServiceAssetReportRequest, ApiCheckout.BatchGetCheckoutSessionsRequest>(value => new ApiCheckout.BatchGetCheckoutSessionsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _checkoutCheckoutSessionService.BatchGetCheckoutSessionsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiIdentity.LookupUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            UserId = (request.Context?.Actor?.UserId ?? ""),
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _identityUserQueryService.LookupUserAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ScheduleLicenseAssignmentExpiryRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ScheduleLicenseAssignmentExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.ServiceAsset ?? new ApiItsm.ServiceAsset()).LocatorCase switch
                    {
                        ApiItsm.ServiceAsset.LocatorOneofCase.Address => (reply0.ServiceAsset?.Address?.Locality ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc3 = _licensingLicenseAssignmentLifecycleService.ScheduleLicenseAssignmentExpiryAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ScheduleServiceAssetReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ScheduleServiceAssetReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a service asset.
    /// </summary>
    public override async Task<ApiItsm.ShareServiceAssetReportResponse> ShareServiceAssetReport(ApiItsm.ShareServiceAssetReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareServiceAssetReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", request.ServiceAssetId,
            "ShareServiceAssetReport", "accepted", token);
        var input0 = new ApiLicensing.EscalateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.EscalateEntitlementRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await SendShareServiceAssetReport0Async(input0, token);
        var input1 = new ApiDrivers.CalculateDriverRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Driver = new ApiDrivers.Driver
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _driversDriverCalculationService.CalculateDriverAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.ShareServiceAssetReportResponse
        {
            Outcome = new ApiItsm.ShareServiceAssetReportResponse.Types.Outcome
            {
                Message = (reply1.Result?.Message ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Name = (reply0.Entitlement?.Name ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                ServiceAssetId = (request.ServiceAssetId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "ShareServiceAssetReport", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of service assets as they happen.
    /// </summary>
    public override async Task StreamServiceAssetMetrics(ApiItsm.StreamServiceAssetMetricsRequest request, IServerStreamWriter<ApiItsm.StreamServiceAssetMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamServiceAssetMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "service_asset", "batch",
            "StreamServiceAssetMetrics", "accepted", token);
        var input0 = new ApiDocuments.ListDocumentFolderVersionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ListDocumentFolderVersionsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Filter?.OwnerId ?? ""),
                Query = (request.Filter?.Query ?? ""),
                Region = (request.Filter?.Region ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ListDocumentFolderVersionsRequest>();
        using var rpc0 = _documentsDocumentFolderHistoryService.ListDocumentFolderVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.StreamServiceAssetMetricsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "service_asset",
            "audit", "StreamServiceAssetMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiLicensing.EscalateEntitlementResponse> SendShareServiceAssetReport0Async(ApiLicensing.EscalateEntitlementRequest request, CancellationToken token)
    {
        return await _serviceAssetReportingEndpointSendShareServiceAssetReport0Port.SendAsync(request, token);
    }
}
