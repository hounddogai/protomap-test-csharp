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
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class JobOpeningQueryEndpoint
{
    /// <summary>
    /// Lists job openings a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.BrowseJobOpeningsResponse> BrowseJobOpenings(ApiRecruiting.BrowseJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "BrowseJobOpenings", "accepted", token);
        var input0 = new ApiRecruiting.ValidateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ValidateJobOpeningRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingJobOpeningValidationService.ValidateJobOpeningAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiRecruiting.EstimateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Bank = new ApiRecruiting.OfferBankSettlement
                {
                    Iban = (reply0.JobOpening ?? new ApiRecruiting.JobOpening()).PaymentCase switch
                    {
                        ApiRecruiting.JobOpening.SettlementOneofCase.Bank => (reply0.JobOpening?.Bank?.Iban ?? ""),
                        _ => ""
                    },
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply0.JobOpening?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingOfferCalculationService.EstimateOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.BrowseJobOpeningsResponse
        {
            JobOpenings = { new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "BrowseJobOpenings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Counts job openings by status.
    /// </summary>
    public override async Task<ApiRecruiting.CountJobOpeningsResponse> CountJobOpenings(ApiRecruiting.CountJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CountJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "CountJobOpenings", "accepted", token);
        var input0 = new ApiCorporateevents.EstimateCorporateEventRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
            Details = new ApiCorporateevents.EstimateCorporateEventRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _corporateeventsCorporateEventCalculationService.EstimateCorporateEventAsync);
        var response = new ApiRecruiting.CountJobOpeningsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "CountJobOpenings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching job opening.
    /// </summary>
    public override async Task ExportJobOpenings(ApiRecruiting.ExportJobOpeningsRequest request, IServerStreamWriter<ApiRecruiting.ExportJobOpeningsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "ExportJobOpenings", "accepted", token);
        var input0 = new ApiRecruiting.CheckOfferEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        using var rpc0 = _sendExportJobOpenings0(input0, new CallOptions(cancellationToken: token));
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.NormalizeInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Offer?.Audit?.Etag ?? ""),
                },
                Location = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.Offer?.BillingAddress?.CountryCode ?? ""),
                    PostalCode = (reply0.Offer?.BillingAddress?.PostalCode ?? ""),
                    Region = (reply0.Offer?.BillingAddress?.Region ?? ""),
                },
            },
        };
        var named1 = _registry.Get<ApiRecruiting.InterviewValidationService.InterviewValidationServiceClient>("acme.recruiting.v1.InterviewValidationService");
        using var rpc1 = named1.NormalizeInterviewAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLicensing.PreviewEntitlementRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Entitlement = new ApiLicensing.Entitlement
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Offer?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.Interview?.Audit?.Etag ?? ""),
                    UpdatedBy = (reply0.Offer?.Audit?.UpdatedBy ?? ""),
                },
            },
        };
        var descriptor2 = new Method<ApiLicensing.PreviewEntitlementRequest, ApiLicensing.PreviewEntitlementResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".EntitlementValidationService", "PreviewEntitlement",
            Wire.Marshaller<ApiLicensing.PreviewEntitlementRequest>(), Wire.Marshaller<ApiLicensing.PreviewEntitlementResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.ExportJobOpeningsResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                BillingAddress = (reply0.Offer?.BillingAddress ?? new ApiCommon.PostalAddress()),
                Metadata = { (reply0.Offer?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Format ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "ExportJobOpenings", (request.Format ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.LookupJobOpeningResponse> LookupJobOpening(ApiRecruiting.LookupJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling LookupJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "LookupJobOpening", "accepted", token);
        var response = new ApiRecruiting.LookupJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.PeekJobOpeningResponse> PeekJobOpening(ApiRecruiting.PeekJobOpeningRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.JobOpeningId, "job_opening_id");
        _logger.LogInformation("Handling PeekJobOpening for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "PeekJobOpening", "accepted", token);
        var response = new ApiRecruiting.PeekJobOpeningResponse
        {
            JobOpening = new ApiRecruiting.JobOpening
            {
                JobOpeningId = (request.JobOpeningId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several job openings at once.
    /// </summary>
    public override async Task<ApiRecruiting.ResolveJobOpeningsResponse> ResolveJobOpenings(ApiRecruiting.ResolveJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "ResolveJobOpenings", "accepted", token);
        var input0 = new ApiRecruiting.ValidateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.ValidateJobOpeningRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiRecruiting.ValidateJobOpeningRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _recruitingJobOpeningValidationService.ValidateJobOpeningAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.ResolveJobOpeningsResponse
        {
            JobOpenings = { new ApiRecruiting.JobOpening
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.JobOpening?.Audit?.Etag ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.JobOpening?.BillingAddress?.Recipient ?? ""),
                },
            } },
        };
        return response;
    }
    /// <summary>
    /// Lists job openings a page at a time.
    /// </summary>
    public override async Task<ApiRecruiting.SuggestJobOpeningsResponse> SuggestJobOpenings(ApiRecruiting.SuggestJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", "batch",
            "SuggestJobOpenings", "accepted", token);
        var profile0 = new MappingProfile<ApiRecruiting.SuggestJobOpeningsRequest, ApiDocuments.CheckDocumentEligibilityRequest>(value => new ApiDocuments.CheckDocumentEligibilityRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Document = new ApiDocuments.Document
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _documentsDocumentValidationService.CheckDocumentEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiRecruiting.SuggestJobOpeningsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SuggestJobOpenings", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a job opening.
    /// </summary>
    public override async Task<ApiRecruiting.SummarizeJobOpeningsResponse> SummarizeJobOpenings(ApiRecruiting.SummarizeJobOpeningsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeJobOpenings for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "job_opening", request.JobOpeningId,
            "SummarizeJobOpenings", "accepted", token);
        var input0 = new ApiRecruiting.RecalculateOffersRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Lines = { new ApiRecruiting.OfferLine
                {
                    Attributes = { (request.Details?.Filters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _recruitingOfferCalculationService.RecalculateOffersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.ValidateJobOpeningRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            JobOpening = new ApiRecruiting.JobOpening
            {
                Bank = new ApiRecruiting.JobOpeningBankSettlement
                {
                    AccountHolder = (reply0.Offer ?? new ApiRecruiting.Offer()).PaymentCase switch
                    {
                        ApiRecruiting.Offer.SettlementOneofCase.Bank => (reply0.Offer?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _recruitingJobOpeningValidationService.ValidateJobOpeningAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SummarizeJobOpeningsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.JobOpeningId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "job_opening",
            "audit", "SummarizeJobOpenings", (request.JobOpeningId ?? ""), token);
        return response;
    }
}
