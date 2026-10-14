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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiMetrics = Acme.Metrics.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiProcurement = Acme.Procurement.V1;
using ApiSecrets = Acme.Secrets.V1;
using ApiTravel = Acme.Travel.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public sealed partial class VisitorPassReportingEndpoint
{
    /// <summary>
    /// Counts visitor passes by status.
    /// </summary>
    public override async Task<ApiWorkplaces.CountVisitorPassTotalsResponse> CountVisitorPassTotals(ApiWorkplaces.CountVisitorPassTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountVisitorPassTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", "batch",
            "CountVisitorPassTotals", "accepted", token);
        var input0 = new ApiTravel.PullTravelPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.PullTravelPoliciesRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            TravelPolicy = new ApiTravel.TravelPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _travelTravelPolicySyncService.PullTravelPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiWorkplaces.CountVisitorPassTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "CountVisitorPassTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.DeleteVisitorPassReportResponse> DeleteVisitorPassReport(ApiWorkplaces.DeleteVisitorPassReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        _logger.LogInformation("Handling DeleteVisitorPassReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "DeleteVisitorPassReport", "accepted", token);
        var input0 = new ApiWorkplaces.ScoreDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _workplacesDeskValidationService.ScoreDeskAsync);
        var input1 = new ApiEmployees.ScoreEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc1 = _sendDeleteVisitorPassReport1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiWorkplaces.DeleteVisitorPassReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPassId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "DeleteVisitorPassReport", (request.VisitorPassId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.FetchVisitorPassReportResponse> FetchVisitorPassReport(ApiWorkplaces.FetchVisitorPassReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.VisitorPassId, "visitor_pass_id");
        _logger.LogInformation("Handling FetchVisitorPassReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "FetchVisitorPassReport", "accepted", token);
        var response = new ApiWorkplaces.FetchVisitorPassReportResponse
        {
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.GenerateVisitorPassReportResponse> GenerateVisitorPassReport(ApiWorkplaces.GenerateVisitorPassReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateVisitorPassReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "GenerateVisitorPassReport", "accepted", token);
        var input0 = new ApiWorkplaces.GetVisitorPassSyncStateRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            VisitorPassId = (request.VisitorPassId ?? ""),
        };
        var named0 = _registry.Get<ApiWorkplaces.VisitorPassSyncService.VisitorPassSyncServiceClient>("acme.workplaces.v1.VisitorPassSyncService");
        using var rpc0 = named0.GetVisitorPassSyncStateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiWorkplaces.ShareRoomBookingReportRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.ShareRoomBookingReportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _workplacesRoomBookingReportingService.ShareRoomBookingReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.CheckEmploymentContractEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.VisitorPass?.Audit?.Etag ?? ""),
                },
            },
        };
        var command2 = new DispatchCommand<ApiEmployees.CheckEmploymentContractEligibilityRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _employeesEmploymentContractValidationService.CheckEmploymentContractEligibilityAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiWorkplaces.GenerateVisitorPassReportRequest, ApiEmployees.BrowseEmployeesRequest>(value => new ApiEmployees.BrowseEmployeesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.BrowseEmployeesRequest.Types.Filter
            {
                CreatedAfter = (reply0.VisitorPass?.Audit?.CreatedAt ?? new Timestamp()),
                Region = (reply1.RoomBooking?.Location?.Region ?? ""),
            },
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _employeesEmployeeQueryService.BrowseEmployeesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.GenerateVisitorPassReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "GenerateVisitorPassReport", (request.Details?.Format ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists visitor passes a page at a time.
    /// </summary>
    public override async Task<ApiWorkplaces.ListVisitorPassReportsResponse> ListVisitorPassReports(ApiWorkplaces.ListVisitorPassReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListVisitorPassReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", "batch",
            "ListVisitorPassReports", "accepted", token);
        var input0 = new ApiPersonalization.SummarizeProfilesRequest
        {
            Annotations = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPersonalization.SummarizeProfilesRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc0 = _personalizationProfileQueryService.SummarizeProfilesAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiProcurement.QuotePurchaseRequisitionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiProcurement.QuotePurchaseRequisitionRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            PurchaseRequisition = new ApiProcurement.PurchaseRequisition
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Metadata = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var rpc1 = _procurementPurchaseRequisitionCalculationService.QuotePurchaseRequisitionAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiWorkplaces.ListVisitorPassReportsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ListVisitorPassReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ScheduleVisitorPassReportResponse> ScheduleVisitorPassReport(ApiWorkplaces.ScheduleVisitorPassReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleVisitorPassReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "ScheduleVisitorPassReport", "accepted", token);
        var input0 = new ApiDocuments.CompareDocumentVersionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CompareDocumentVersionsRequest.Types.Details
            {
                AuthorEmail = (request.Context?.Actor?.Email ?? ""),
                Snapshot = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Document = new ApiDocuments.Document
            {
                Lines = { new ApiDocuments.DocumentLine
                {
                    Attributes = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _documentsDocumentHistoryService.CompareDocumentVersionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiEmployees.ValidatePositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input1.Context.Baggage.Clear();
        input1.Context.Baggage.Add(labels1);
        using var rpc1 = _employeesPositionValidationService.ValidatePositionAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSecrets.BrowseSecretsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSecrets.BrowseSecretsRequest.Types.Filter
            {
                Region = (reply0.Document?.BillingAddress?.Region ?? ""),
            },
        };
        var packed2 = Any.Pack(input2);
        input2 = packed2.Unpack<ApiSecrets.BrowseSecretsRequest>();
        using var rpc2 = _secretsSecretQueryService.BrowseSecretsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ScheduleVisitorPassReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.VisitorPassId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ScheduleVisitorPassReport", (request.VisitorPassId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a visitor pass.
    /// </summary>
    public override async Task<ApiWorkplaces.ShareVisitorPassReportResponse> ShareVisitorPassReport(ApiWorkplaces.ShareVisitorPassReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareVisitorPassReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", request.VisitorPassId,
            "ShareVisitorPassReport", "accepted", token);
        var input0 = new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiWorkplaces.EstimateDeskRequest>(json0);
        using var rpc0 = _workplacesDeskCalculationService.EstimateDeskAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.ScheduleTravelPolicyReportRequest();
        input1.Annotations.Add((request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()));
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Details = new ApiTravel.ScheduleTravelPolicyReportRequest.Types.Details
        {
            Filters = { (reply0.Desk?.Specs ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        input1.Reason = (request.Reason ?? "");
        using var rpc1 = _travelTravelPolicyReportingService.ScheduleTravelPolicyReportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiWorkplaces.CountBuildingsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiWorkplaces.CountBuildingsRequest.Types.Filter
            {
                CreatedAfter = (reply0.Desk?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Details?.TemplateId ?? ""),
                Region = (reply0.Desk ?? new ApiWorkplaces.Desk()).LocatorCase switch
                {
                    ApiWorkplaces.Desk.LocatorOneofCase.Address => (reply0.Desk?.Address?.Region ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc2 = _workplacesBuildingQueryService.CountBuildingsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiWorkplaces.ShareVisitorPassReportResponse
        {
            Outcome = new ApiWorkplaces.ShareVisitorPassReportResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                VisitorPassId = (request.VisitorPassId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Recipient?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "ShareVisitorPassReport", (request.Details?.Recipient?.PreferredChannel ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of visitor passes as they happen.
    /// </summary>
    public override async Task StreamVisitorPassMetrics(ApiWorkplaces.StreamVisitorPassMetricsRequest request, IServerStreamWriter<ApiWorkplaces.StreamVisitorPassMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamVisitorPassMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass", "batch",
            "StreamVisitorPassMetrics", "accepted", token);
        var input0 = new ApiWorkplaces.CheckVisitorPassEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.CheckVisitorPassEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _workplacesVisitorPassValidationService.CheckVisitorPassEligibilityAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiMetrics.BrowseMetricSeriesesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiMetrics.BrowseMetricSeriesesRequest.Types.Filter
            {
                CreatedAfter = (request.Filter?.CreatedAfter ?? new Timestamp()),
                Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
            },
        };
        var reply1 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _metricsMetricSeriesQueryService.BrowseMetricSeriesesAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiEmployees.LintEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.LintEmploymentContractsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.VisitorPass?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var reply2 = await Workflow.SendAsync(input2, token, _employeesEmploymentContractValidationService.LintEmploymentContractsAsync);
        var input3 = new ApiWorkplaces.ScoreDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Desk = new ApiWorkplaces.Desk
            {
                Address = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply2.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.VisitorPass?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        using var rpc3 = _sendStreamVisitorPassMetrics3(input3, new CallOptions(cancellationToken: token));
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiWorkplaces.StreamVisitorPassMetricsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Result?.Status ?? ""),
            },
            VisitorPass = new ApiWorkplaces.VisitorPass
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply3.Desk?.Address?.CountryCode ?? ""),
                    Locality = (reply3.Desk?.Address?.Locality ?? ""),
                    Region = (reply2.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.VisitorPass?.Counterparty?.Email ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "visitor_pass",
            "audit", "StreamVisitorPassMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
