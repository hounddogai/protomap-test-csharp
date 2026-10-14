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
using ApiFraud = Acme.Fraud.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Recruiting;

public sealed partial class OfferLifecycleEndpoint
{
    /// <summary>
    /// Activates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ActivateOfferResponse> ActivateOffer(ApiRecruiting.ActivateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling ActivateOffer for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "ActivateOffer", "accepted", token);
        var input0 = new ApiItsm.ListIncidentWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ListIncidentWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Details?.EffectiveAt ?? new Timestamp()),
                OwnerId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        using var rpc0 = _itsmIncidentWorkflowService.ListIncidentWorkflowsAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.PreviewCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    UserId = (request.Details?.NewOwner?.UserId ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.PreviewCandidateRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        using var rpc1 = _recruitingCandidateValidationService.PreviewCandidateAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiRecruiting.ActivateOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.ActivateOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ActivateOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Expires an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ExpireOfferResponse> ExpireOffer(ApiRecruiting.ExpireOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling ExpireOffer for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "ExpireOffer", "accepted", token);
        var input0 = new ApiRecruiting.ValidateCandidateRequest();
        input0.Candidate = new ApiRecruiting.Candidate
        {
            Contact = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.NewOwner?.Email ?? ""),
            },
            RelationshipManager = new ApiIdentity.UserRef
            {
                DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                UserId = (request.Details?.NewOwner?.UserId ?? ""),
            },
        };
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        using var rpc0 = _recruitingCandidateValidationService.ValidateCandidateAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.QuoteSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply0.Candidate?.RelationshipManager?.DisplayName ?? ""),
                    Number = (reply0.Candidate ?? new ApiRecruiting.Candidate()).IdentityDocumentCase switch
                    {
                        ApiRecruiting.Candidate.IdentityDocumentOneofCase.NationalId => (reply0.Candidate?.NationalId?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsSignatureCalculationService.QuoteSignatureAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.SimulateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.NotifyContact?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply2 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingCandidateCalculationService.SimulateCandidateAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.ExpireOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.Candidate?.PrimaryAddress?.PostalCode ?? ""),
                    Recipient = (reply2.Candidate?.PrimaryAddress?.Recipient ?? ""),
                },
                Lines = { new ApiRecruiting.OfferLine
                {
                    Attributes = { (reply1.Signature?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.ExpireOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ExpireOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an offer.
    /// </summary>
    public override async Task<ApiRecruiting.GetOfferLifecycleResponse> GetOfferLifecycle(ApiRecruiting.GetOfferLifecycleRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling GetOfferLifecycle for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "GetOfferLifecycle", "accepted", token);
        var input0 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Context?.Deadline ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Lines = { new ApiEmployees.EmploymentContractLine
                {
                    Attributes = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                } },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _employeesEmploymentContractCalculationService.RecalculateEmploymentContractsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiRecruiting.GetOfferLifecycleResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (reply0.EmploymentContract?.BillingAddress?.Region ?? ""),
                },
                OfferId = (request.OfferId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Renews an offer.
    /// </summary>
    public override async Task<ApiRecruiting.RenewOfferResponse> RenewOffer(ApiRecruiting.RenewOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling RenewOffer for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "RenewOffer", "accepted", token);
        var input0 = new ApiFraud.SimulateRiskAssessmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RiskAssessment = new ApiFraud.RiskAssessment
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.NewOwner?.Email ?? ""),
                    Name = (request.Details?.NotifyContact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.NotifyContact?.PreferredChannel ?? ""),
                },
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _fraudRiskAssessmentCalculationService.SimulateRiskAssessmentAsync);
        var response = new ApiRecruiting.RenewOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.RiskAssessment?.Audit?.UpdatedBy ?? ""),
                },
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.RenewOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "RenewOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Schedules an offer.
    /// </summary>
    public override async Task<ApiRecruiting.ScheduleOfferExpiryResponse> ScheduleOfferExpiry(ApiRecruiting.ScheduleOfferExpiryRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScheduleOfferExpiry for {Tenant}", requestContext.TenantId);
        if (request.Offer is { } record)
        {
            var snapshot = OfferMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFraud.CalculateRiskAssessmentRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            RiskAssessment = new ApiFraud.RiskAssessment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc0 = _fraudRiskAssessmentCalculationService.CalculateRiskAssessmentAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.SimulateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Offer?.Audit?.Etag ?? ""),
                },
                Card = new ApiRecruiting.OfferCardSettlement
                {
                    Network = (request.Offer?.Card?.Network ?? ""),
                },
            },
        };
        using var rpc1 = _recruitingOfferCalculationService.SimulateOfferAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var input2 = new ApiRecruiting.ValidateCandidateRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.Offer?.Audit?.CreatedBy ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor2 = new Method<ApiRecruiting.ValidateCandidateRequest, ApiRecruiting.ValidateCandidateResponse>(
            MethodType.Unary, "acme.recruiting.v1" + ".CandidateValidationService", "ValidateCandidate",
            Wire.Marshaller<ApiRecruiting.ValidateCandidateRequest>(), Wire.Marshaller<ApiRecruiting.ValidateCandidateResponse>());
        using var rpc2 = _invoker.AsyncUnaryCall(descriptor2, null,
            new CallOptions(cancellationToken: token), input2);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiRecruiting.ScheduleOfferExpiryResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply2.Candidate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (reply0.RiskAssessment?.Customer?.Email ?? ""),
                },
                Metadata = { (request.Offer?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Notes = (reply1.Offer?.Notes ?? ""),
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.ScheduleOfferExpiryResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "ScheduleOfferExpiry", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Suspends an offer.
    /// </summary>
    public override async Task<ApiRecruiting.SuspendOfferResponse> SuspendOffer(ApiRecruiting.SuspendOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling SuspendOffer for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "SuspendOffer", "accepted", token);
        var input0 = new ApiLicensing.DelegateEntitlementApprovalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.DelegateEntitlementApprovalRequest.Types.Details
            {
                Approver = new ApiIdentity.UserRef
                {
                    DisplayName = (request.Details?.NewOwner?.DisplayName ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiLicensing.DelegateEntitlementApprovalRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _licensingEntitlementApprovalService.DelegateEntitlementApprovalAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var profile1 = new MappingProfile<ApiRecruiting.SuspendOfferRequest, ApiRecruiting.CheckCandidateEligibilityRequest>(value => new ApiRecruiting.CheckCandidateEligibilityRequest
        {
            Candidate = new ApiRecruiting.Candidate
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Entitlement?.Audit?.Etag ?? ""),
                },
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (value.Details?.NewOwner?.Email ?? ""),
                    UserId = (reply0.Entitlement?.Owner?.UserId ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiRecruiting.CheckCandidateEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Details?.NotifyContact?.Email ?? ""),
                },
            },
        });
        var input1 = profile1.Map(request);
        using var rpc1 = _recruitingCandidateValidationService.CheckCandidateEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiRecruiting.SuspendOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply1.Candidate?.PrimaryAddress?.Locality ?? ""),
                },
                Lines = { new ApiRecruiting.OfferLine
                {
                    Description = (reply0.Entitlement?.Description ?? ""),
                } },
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.SuspendOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Status = (reply0.Outcome?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "SuspendOffer", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Terminates an offer.
    /// </summary>
    public override async Task<ApiRecruiting.TerminateOfferResponse> TerminateOffer(ApiRecruiting.TerminateOfferRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.OfferId, "offer_id");
        _logger.LogInformation("Handling TerminateOffer for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "offer", request.OfferId,
            "TerminateOffer", "accepted", token);
        var input0 = new ApiIdentity.PreviewUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.PreviewUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.NotifyContact?.Phone ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _identityUserValidationService.PreviewUserAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.TerminateOfferToListDocumentFolderExports1(reply0);
        using var rpc1 = _documentsDocumentFolderExportService.ListDocumentFolderExportsAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiEmployees.SummarizeOrgUnitsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        var reply2 = await SendTerminateOffer2Async(input2, token);
        var response = new ApiRecruiting.TerminateOfferResponse
        {
            Offer = new ApiRecruiting.Offer
            {
                Number = (reply0.User?.Passport?.Number ?? ""),
                OfferId = (request.OfferId ?? ""),
            },
            Outcome = new ApiRecruiting.TerminateOfferResponse.Types.Outcome
            {
                EffectiveAt = (request.Details?.EffectiveAt ?? new Timestamp()),
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Details?.ReasonCode ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "offer",
            "audit", "TerminateOffer", (request.Details?.ReasonCode ?? ""), token);
        return response;
    }
    private async Task<ApiEmployees.SummarizeOrgUnitsResponse> SendTerminateOffer2Async(ApiEmployees.SummarizeOrgUnitsRequest request, CancellationToken token)
    {
        return await _offerLifecycleEndpointSendTerminateOffer2Port.SendAsync(request, token);
    }
}
