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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class OfferReportingEndpoint
{
    /// <summary>
    /// Counts offers by status.
    /// </summary>
    public override async Task<ApiRecruiting.CountOfferTotalsResponse> CountOfferTotals(ApiRecruiting.CountOfferTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountOfferTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", "batch",
            "CountOfferTotals", "accepted", token);
        var input0 = new ApiCorporateevents.RejectVenueRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _corporateeventsVenueApprovalService.RejectVenueAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.CountOfferTotalsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "CountOfferTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an offer.
    /// </summary>
    public override async Task<ApiRecruiting.DeleteOfferReportResponse> DeleteOfferReport(ApiRecruiting.DeleteOfferReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling DeleteOfferReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "DeleteOfferReport", "accepted", token);
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiEmployees.ExplainEmploymentContractCalculationRequest>();
        using var rpc0 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.DeleteOfferReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "DeleteOfferReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an offer.
    /// </summary>
    public override async Task<ApiRecruiting.FetchOfferReportResponse> FetchOfferReport(ApiRecruiting.FetchOfferReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling FetchOfferReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "FetchOfferReport", "accepted", token);
        var input0 = new ApiIdentity.UpdateUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiIdentity.UpdateUserRequest>(json0);
        using var rpc0 = _identityUserService.UpdateUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.FetchOfferReportResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
                },
                OfferId = (request.OfferId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.GenerateOfferReportResponse> GenerateOfferReport(ApiRecruiting.GenerateOfferReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateOfferReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "GenerateOfferReport", "accepted", token);
        var input0 = new ApiRecruiting.PushInterviewRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.PushInterviewRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            Interview = new ApiRecruiting.Interview
            {
                Participants = { new ApiRecruiting.InterviewParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc0 = _recruitingInterviewSyncService.PushInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ExplainCandidateCalculationRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.Interview?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _recruitingCandidateCalculationService.ExplainCandidateCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.GenerateOfferReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.OfferId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "GenerateOfferReport", (request.OfferId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists offers a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.ListOfferReportsResponse> ListOfferReports(ApiRecruiting.ListOfferReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListOfferReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", "batch",
            "ListOfferReports", "accepted", token);
        var response = new ApiRecruiting.ListOfferReportsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ListOfferReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleOfferReportResponse> ScheduleOfferReport(ApiRecruiting.ScheduleOfferReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleOfferReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "ScheduleOfferReport", "accepted", token);
        var input0 = new ApiLedger.NormalizeLedgerAccountRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLedger.NormalizeLedgerAccountRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _ledgerLedgerAccountValidationService.NormalizeLedgerAccountAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.BatchGetDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc1 = _documentsDocumentService.BatchGetDocumentsAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.ScheduleInterviewReportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ScheduleInterviewReportRequest.Types.Details
            {
                Filters = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Format = (request.Details?.Format ?? ""),
                RecipientEmails = { (request.Details?.RecipientEmails ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        using var rpc2 = _recruitingInterviewReportingService.ScheduleInterviewReportAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var response = new ApiRecruiting.ScheduleOfferReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ScheduleOfferReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of offers as they happen.
    /// </summary>
    public override async Task StreamOfferMetrics(ApiRecruiting.StreamOfferMetricsRequest request, IServerStreamWriter<ApiRecruiting.StreamOfferMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamOfferMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", "batch",
            "StreamOfferMetrics", "accepted", token);
        var input0 = new ApiEmployees.ExportOrgUnitsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiEmployees.ExportOrgUnitsRequest.Types.Filter
            {
                Labels = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        using var stream0 = _employeesOrgUnitQueryService.ExportOrgUnits(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportOrgUnits frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiIdentity.BatchGetUsersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var named1 = _registry.Get<ApiIdentity.UserService.UserServiceClient>("acme.identity.v1.UserService");
        using var rpc1 = named1.BatchGetUsersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiCorporateevents.CommentVenueApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCorporateevents.CommentVenueApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Venue = new ApiCorporateevents.Venue
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiCorporateevents.CommentVenueApprovalRequest, ApiCorporateevents.CommentVenueApprovalResponse>(
            MethodType.Unary, "acme.corporateevents.v1" + ".VenueApprovalService", "CommentVenueApproval",
            Wire.Marshaller<ApiCorporateevents.CommentVenueApprovalRequest>(), Wire.Marshaller<ApiCorporateevents.CommentVenueApprovalResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiCorporateevents.CommentVenueApprovalRequest>("acme.recruiting.v1.OfferReportingService/StreamOfferMetrics/2", input2.Clone()), token);
        var response = new ApiRecruiting.StreamOfferMetricsResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply2.Venue?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "StreamOfferMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
