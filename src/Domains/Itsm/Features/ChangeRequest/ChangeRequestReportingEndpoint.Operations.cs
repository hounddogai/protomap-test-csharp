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
using ApiLicensing = Acme.Licensing.V1;
using ApiMaintenance = Acme.Maintenance.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Itsm;

public sealed partial class ChangeRequestReportingEndpoint
{
    /// <summary>
    /// Counts change requests by status.
    /// </summary>
    public override async Task<ApiItsm.CountChangeRequestTotalsResponse> CountChangeRequestTotals(ApiItsm.CountChangeRequestTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountChangeRequestTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "CountChangeRequestTotals", "accepted", token);
        var input0 = new ApiItsm.RecalculateIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.RecalculateIncidentsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Incident = new ApiItsm.Incident
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                Customer = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _itsmIncidentCalculationService.RecalculateIncidentsAsync);
        var response = new ApiItsm.CountChangeRequestTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "CountChangeRequestTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a change request.
    /// </summary>
    public override async Task<ApiItsm.DeleteChangeRequestReportResponse> DeleteChangeRequestReport(ApiItsm.DeleteChangeRequestReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling DeleteChangeRequestReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "DeleteChangeRequestReport", "accepted", token);
        var input0 = new ApiItsm.CheckServiceAssetEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        using var rpc0 = _sendDeleteChangeRequestReport0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.EstimateProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ServiceAsset?.Owner?.UserId ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiItsm.ProblemCalculationService.ProblemCalculationServiceClient>("acme.itsm.v1.ProblemCalculationService");
        using var rpc1 = named1.EstimateProblemAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.DeleteChangeRequestReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Outcome?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.ChangeRequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "DeleteChangeRequestReport", (request.ChangeRequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a change request.
    /// </summary>
    public override async Task<ApiItsm.FetchChangeRequestReportResponse> FetchChangeRequestReport(ApiItsm.FetchChangeRequestReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.ChangeRequestId, "change_request_id");
        _logger.LogInformation("Handling FetchChangeRequestReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "FetchChangeRequestReport", "accepted", token);
        var response = new ApiItsm.FetchChangeRequestReportResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                ChangeRequestId = (request.ChangeRequestId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a change request.
    /// </summary>
    public override async Task<ApiItsm.GenerateChangeRequestReportResponse> GenerateChangeRequestReport(ApiItsm.GenerateChangeRequestReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateChangeRequestReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "GenerateChangeRequestReport", "accepted", token);
        var input0 = new ApiItsm.QuoteServiceAssetRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiItsm.QuoteServiceAssetRequest, ApiItsm.QuoteServiceAssetResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".ServiceAssetCalculationService", "QuoteServiceAsset",
            Wire.Marshaller<ApiItsm.QuoteServiceAssetRequest>(), Wire.Marshaller<ApiItsm.QuoteServiceAssetResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ExplainProblemCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.ExplainProblemCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
                },
            },
            Problem = new ApiItsm.Problem
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiItsm.ProblemComment
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Customer = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.ServiceAsset?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var command1 = new DispatchCommand<ApiItsm.ExplainProblemCalculationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _itsmProblemCalculationService.ExplainProblemCalculationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiItsm.ExportIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ExportIncidentsRequest.Types.Filter
            {
                CreatedAfter = (reply1.Problem?.Audit?.UpdatedAt ?? new Timestamp()),
                OwnerId = (reply1.Problem?.Sla?.PolicyId ?? ""),
                Region = (reply0.ServiceAsset?.Address?.Region ?? ""),
            },
            Format = (request.Details?.Format ?? ""),
        };
        using var stream2 = _itsmIncidentQueryService.ExportIncidents(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportIncidents frame {Type}", change2.GetType().Name);
        }
        var response = new ApiItsm.GenerateChangeRequestReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.Cron ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "GenerateChangeRequestReport", (request.Details?.Cron ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists change requests a page at a time.
    /// </summary>
    public override async Task<ApiItsm.ListChangeRequestReportsResponse> ListChangeRequestReports(ApiItsm.ListChangeRequestReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListChangeRequestReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "ListChangeRequestReports", "accepted", token);
        var input0 = new ApiDocuments.CreateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _documentsDocumentService.CreateDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiItsm.ListChangeRequestReportsResponse
        {
            ChangeRequests = { new ApiItsm.ChangeRequest
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Document?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply0.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ListChangeRequestReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a change request.
    /// </summary>
    public override async Task<ApiItsm.ScheduleChangeRequestReportResponse> ScheduleChangeRequestReport(ApiItsm.ScheduleChangeRequestReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleChangeRequestReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "ScheduleChangeRequestReport", "accepted", token);
        var input0 = request.ScheduleChangeRequestReportToListSignatureExports0();
        using var rpc0 = _documentsSignatureExportService.ListSignatureExportsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.GenerateLicenseAssignmentReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.GenerateLicenseAssignmentReportRequest.Types.Details
            {
                Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            },
        };
        var reply1 = await SendScheduleChangeRequestReport1Async(input1, token);
        var response = new ApiItsm.ScheduleChangeRequestReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
            Summary = { (reply1.Summary ?? new Google.Protobuf.Collections.MapField<string, string>()) },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ScheduleChangeRequestReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a change request.
    /// </summary>
    public override async Task<ApiItsm.ShareChangeRequestReportResponse> ShareChangeRequestReport(ApiItsm.ShareChangeRequestReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareChangeRequestReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", request.ChangeRequestId,
            "ShareChangeRequestReport", "accepted", token);
        var input0 = new ApiItsm.RecalculateServiceAssetsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ServiceAsset = new ApiItsm.ServiceAsset
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _itsmServiceAssetCalculationService.RecalculateServiceAssetsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiMaintenance.ResolveWorkOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed1 = Any.Pack(input1);
        input1 = packed1.Unpack<ApiMaintenance.ResolveWorkOrdersRequest>();
        using var rpc1 = _maintenanceWorkOrderQueryService.ResolveWorkOrdersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.CheckPositionEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Position = new ApiEmployees.Position
            {
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.ServiceAsset?.Owner?.UserId ?? ""),
                },
            },
        };
        var json2 = JsonFormatter.Default.Format(input2);
        input2 = JsonParser.Default.Parse<ApiEmployees.CheckPositionEligibilityRequest>(json2);
        using var rpc2 = _employeesPositionValidationService.CheckPositionEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiItsm.CreateIncidentRequest();
        input3.Context = (request.Context ?? new ApiCommon.RequestContext());
        input3.Incident = new ApiItsm.Incident
        {
            Assignee = new ApiIdentity.UserRef
            {
                Email = (reply0.ServiceAsset?.Owner?.Email ?? ""),
            },
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (reply0.ServiceAsset?.Audit?.UpdatedBy ?? ""),
            },
            Requester = new ApiCommon.ContactInfo
            {
                Email = (reply2.Position?.Owner?.Email ?? ""),
            },
        };
        using var rpc3 = _itsmIncidentService.CreateIncidentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiItsm.ShareChangeRequestReportResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    DisplayName = (reply3.Incident?.Assignee?.DisplayName ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.ServiceAsset?.Audit?.CreatedAt ?? new Timestamp()),
                },
                ChangeRequestId = (request.ChangeRequestId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
                Tags = { (reply0.ServiceAsset?.Tags ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Outcome = new ApiItsm.ShareChangeRequestReportResponse.Types.Outcome
            {
                NextSteps = { (reply2.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply2.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "ShareChangeRequestReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of change requests as they happen.
    /// </summary>
    public override async Task StreamChangeRequestMetrics(ApiItsm.StreamChangeRequestMetricsRequest request, IServerStreamWriter<ApiItsm.StreamChangeRequestMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamChangeRequestMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "change_request", "batch",
            "StreamChangeRequestMetrics", "accepted", token);
        var input0 = new ApiItsm.PreviewProblemRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiItsm.PreviewProblemRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
            Problem = new ApiItsm.Problem
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.Subscriber?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _itsmProblemValidationService.PreviewProblemAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDevices.ScheduleComplianceScanExpiryRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiRecruiting.CandidateRef
                {
                    DisplayName = (reply0.Problem?.Assignee?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.ScheduleComplianceScanExpiryRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Problem?.Requester?.Email ?? ""),
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (reply0.Problem?.Requester?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _devicesComplianceScanLifecycleService.ScheduleComplianceScanExpiryAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiItsm.StreamChangeRequestMetricsResponse
        {
            ChangeRequest = new ApiItsm.ChangeRequest
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply1.ComplianceScan?.Subject?.Email ?? ""),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.Region ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "change_request",
            "audit", "StreamChangeRequestMetrics", (request.Filter?.Region ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiLicensing.GenerateLicenseAssignmentReportResponse> SendScheduleChangeRequestReport1Async(ApiLicensing.GenerateLicenseAssignmentReportRequest request, CancellationToken token)
    {
        return await _changeRequestReportingEndpointSendScheduleChangeRequestReport1Port.SendAsync(request, token);
    }
}
