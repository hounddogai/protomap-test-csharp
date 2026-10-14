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
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPayments = Acme.Payments.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSurveys = Acme.Surveys.V1;

namespace Acme.Workforce.Licensing;

public sealed partial class LicenseAssignmentReportingEndpoint
{
    /// <summary>
    /// Counts license assignments by status.
    /// </summary>
    public override async Task<ApiLicensing.CountLicenseAssignmentTotalsResponse> CountLicenseAssignmentTotals(ApiLicensing.CountLicenseAssignmentTotalsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountLicenseAssignmentTotals for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", "batch",
            "CountLicenseAssignmentTotals", "accepted", token);
        var response = new ApiLicensing.CountLicenseAssignmentTotalsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "CountLicenseAssignmentTotals", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.DeleteLicenseAssignmentReportResponse> DeleteLicenseAssignmentReport(ApiLicensing.DeleteLicenseAssignmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling DeleteLicenseAssignmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "DeleteLicenseAssignmentReport", "accepted", token);
        var input0 = new ApiLearning.SimulateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Counterparty = new ApiRecruiting.CandidateRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command0 = new DispatchCommand<ApiLearning.SimulateCertificationRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _learningCertificationCalculationService.SimulateCertificationAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiLicensing.DeleteLicenseAssignmentReportRequest, ApiLicensing.LintEntitlementsRequest>(value => new ApiLicensing.LintEntitlementsRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Certification?.Audit?.CreatedAt ?? new Timestamp()),
                    CreatedBy = (reply0.Certification?.Audit?.CreatedBy ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _licensingEntitlementValidationService.LintEntitlementsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.ExplainEntitlementDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Certification?.Audit?.Etag ?? ""),
                },
            },
        };
        input2 = PropertyMap.Copy(input2);
        using var rpc2 = _licensingEntitlementValidationService.ExplainEntitlementDecisionAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = request.DeleteLicenseAssignmentReportToResolveSoftwareLicenses3();
        using var rpc3 = _licensingSoftwareLicenseQueryService.ResolveSoftwareLicensesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.DeleteLicenseAssignmentReportResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "DeleteLicenseAssignmentReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.FetchLicenseAssignmentReportResponse> FetchLicenseAssignmentReport(ApiLicensing.FetchLicenseAssignmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LicenseAssignmentId, "license_assignment_id");
        _logger.LogInformation("Handling FetchLicenseAssignmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "FetchLicenseAssignmentReport", "accepted", token);
        var input0 = new ApiLicensing.FindEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.FindEntitlementsRequest.Types.Filter
            {
                CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
                OwnerId = (request.LicenseAssignmentId ?? ""),
            },
        };
        var reply0 = await SendFetchLicenseAssignmentReport0Async(input0, token);
        var response = new ApiLicensing.FetchLicenseAssignmentReportResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Generates a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.GenerateLicenseAssignmentReportResponse> GenerateLicenseAssignmentReport(ApiLicensing.GenerateLicenseAssignmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling GenerateLicenseAssignmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "GenerateLicenseAssignmentReport", "accepted", token);
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
            Reason = (request.Reason ?? ""),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _documentsDocumentValidationService.ValidateDocumentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.GenerateLicenseAssignmentReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.LicenseAssignmentId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "GenerateLicenseAssignmentReport", (request.LicenseAssignmentId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists license assignments a page at a time.
    /// </summary>
    public override async Task<ApiLicensing.ListLicenseAssignmentReportsResponse> ListLicenseAssignmentReports(ApiLicensing.ListLicenseAssignmentReportsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ListLicenseAssignmentReports for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", "batch",
            "ListLicenseAssignmentReports", "accepted", token);
        var input0 = new ApiLicensing.BatchGetSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLicensing.BatchGetSoftwareLicensesRequest>();
        using var rpc0 = _licensingSoftwareLicenseService.BatchGetSoftwareLicensesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ListLicenseAssignmentReportsResponse
        {
            PageInfo = new ApiCommon.PageInfo
            {
                NextPageToken = (reply0.PageInfo?.NextPageToken ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ListLicenseAssignmentReports", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ScheduleLicenseAssignmentReportResponse> ScheduleLicenseAssignmentReport(ApiLicensing.ScheduleLicenseAssignmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleLicenseAssignmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "ScheduleLicenseAssignmentReport", "accepted", token);
        var input0 = new ApiPayments.EstimatePaymentRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiPayments.EstimatePaymentRequest>(json0);
        using var rpc0 = _paymentsPaymentCalculationService.EstimatePaymentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLicensing.ValidateEntitlementRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Entitlement = new ApiLicensing.Entitlement
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (reply0.Payment?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _licensingEntitlementValidationService.ValidateEntitlementAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiSurveys.WatchSurveysRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSurveys.WatchSurveysRequest.Types.Filter
            {
                Region = (reply0.Payment?.BillingAddress?.Region ?? ""),
            },
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply1.Entitlement?.Owner?.Email ?? ""),
            },
        };
        using var stream2 = _surveysSurveyService.WatchSurveys(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchSurveys frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiDocuments.NormalizeDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NormalizeDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Entitlement?.Owner?.Email ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Payment?.Audit?.Etag ?? ""),
                },
                Lines = { new ApiDocuments.DocumentLine
                {
                    Tax = new ApiDocuments.DocumentLineTax
                    {
                        Amount = (reply1.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply1.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc3 = _documentsDocumentValidationService.NormalizeDocumentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.ScheduleLicenseAssignmentReportResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ScheduleLicenseAssignmentReport", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Shares a license assignment.
    /// </summary>
    public override async Task<ApiLicensing.ShareLicenseAssignmentReportResponse> ShareLicenseAssignmentReport(ApiLicensing.ShareLicenseAssignmentReportRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ShareLicenseAssignmentReport for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", request.LicenseAssignmentId,
            "ShareLicenseAssignmentReport", "accepted", token);
        var input0 = new ApiLicensing.SimulateEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    Reason = (request.Reason ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Recipient?.Email ?? ""),
                },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _licensingEntitlementCalculationService.SimulateEntitlementAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiLicensing.PreviewSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.PreviewSoftwareLicenseRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Entitlement?.Owner?.Email ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _licensingSoftwareLicenseValidationService.PreviewSoftwareLicenseAsync);
        await _bus.PublishAsync(new TransportNotification<ApiLicensing.PreviewSoftwareLicenseRequest>("acme.licensing.v1.LicenseAssignmentReportingService/ShareLicenseAssignmentReport/1", input1.Clone()), token);
        var input2 = new ApiDocuments.ValidateDocumentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ValidateDocumentRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Recipient?.Phone ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.SoftwareLicense?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Token = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsDocumentValidationService.ValidateDocumentAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.ScoreLicenseAssignmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.ScoreLicenseAssignmentRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply2.Document?.BillingAddress?.Recipient ?? ""),
                },
            },
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Card = new ApiLicensing.LicenseAssignmentCardSettlement
                {
                    HolderName = (reply1.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (reply1.SoftwareLicense?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (reply0.Entitlement?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiLicensing.LicenseAssignmentLine
                {
                    Tax = new ApiLicensing.LicenseAssignmentLineTax
                    {
                        Amount = (reply0.Entitlement ?? new ApiLicensing.Entitlement()).ActionCase switch
                        {
                            ApiLicensing.Entitlement.ActionOneofCase.Discount => (reply0.Entitlement?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc3 = _licensingLicenseAssignmentValidationService.ScoreLicenseAssignmentAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.ShareLicenseAssignmentReportResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.Entitlement?.Audit?.CreatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply3.LicenseAssignment?.BillingAddress?.Locality ?? ""),
                },
                LicenseAssignmentId = (request.LicenseAssignmentId ?? ""),
            },
            Outcome = new ApiLicensing.ShareLicenseAssignmentReportResponse.Types.Outcome
            {
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.TemplateId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "ShareLicenseAssignmentReport", (request.Details?.TemplateId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of license assignments as they happen.
    /// </summary>
    public override async Task StreamLicenseAssignmentMetrics(ApiLicensing.StreamLicenseAssignmentMetricsRequest request, IServerStreamWriter<ApiLicensing.StreamLicenseAssignmentMetricsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling StreamLicenseAssignmentMetrics for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment", "batch",
            "StreamLicenseAssignmentMetrics", "accepted", token);
        var input0 = new ApiLicensing.CompareSoftwareLicenseOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.CompareSoftwareLicenseOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                    Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Subscriber?.Phone ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiLicensing.CompareSoftwareLicenseOptionsRequest, ApiLicensing.CompareSoftwareLicenseOptionsResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseCalculationService", "CompareSoftwareLicenseOptions",
            Wire.Marshaller<ApiLicensing.CompareSoftwareLicenseOptionsRequest>(), Wire.Marshaller<ApiLicensing.CompareSoftwareLicenseOptionsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.StreamLicenseAssignmentMetricsResponse
        {
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                LicenseAssignmentId = (reply0.SoftwareLicense?.Holder?.LicenseAssignmentId ?? ""),
                Number = (reply0.SoftwareLicense?.Holder?.Number ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.PreferredChannel ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "license_assignment",
            "audit", "StreamLicenseAssignmentMetrics", (request.Subscriber?.PreferredChannel ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    private async Task<ApiLicensing.FindEntitlementsResponse> SendFetchLicenseAssignmentReport0Async(ApiLicensing.FindEntitlementsRequest request, CancellationToken token)
    {
        return await _licenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port.SendAsync(request, token);
    }
}
