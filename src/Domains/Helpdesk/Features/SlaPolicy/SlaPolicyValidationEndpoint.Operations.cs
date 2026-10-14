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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiFleet = Acme.Fleet.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class SlaPolicyValidationEndpoint
{
    /// <summary>
    /// Checks a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.CheckSlaPolicyEligibilityResponse> CheckSlaPolicyEligibility(ApiHelpdesk.CheckSlaPolicyEligibilityRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CheckSlaPolicyEligibility for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.CloneAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Discount = new ApiAuthorization.AccessPolicyDiscountAction
                {
                    Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                    {
                        ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _authorizationAccessPolicyService.CloneAccessPolicyAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.CheckSlaPolicyEligibilityResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.AccessPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Description = (request.SlaPolicy?.Description ?? ""),
                Parameters = { (reply0.AccessPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "CheckSlaPolicyEligibility", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainSlaPolicyDecisionResponse> ExplainSlaPolicyDecision(ApiHelpdesk.ExplainSlaPolicyDecisionRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainSlaPolicyDecision for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiFleet.ResolveVehiclesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _fleetVehicleQueryService.ResolveVehiclesAsync);
        var input1 = new ApiDevices.ScoreManagedDeviceRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            ManagedDevice = new ApiDevices.ManagedDevice
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                    UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                    Email = (request.Details?.Contact?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendExplainSlaPolicyDecision1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.GetSlaPolicyRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SlaPolicyId = (request.SlaPolicy?.SlaPolicyId ?? ""),
        };
        var named2 = _registry.Get<ApiHelpdesk.SlaPolicyCalculationService.SlaPolicyCalculationServiceClient>("acme.helpdesk.v1.SlaPolicyCalculationService");
        using var rpc2 = named2.GetSlaPolicyRatesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiLicensing.SimulateSoftwareLicenseRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.ManagedDevice?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                },
            },
        };
        var descriptor3 = new Method<ApiLicensing.SimulateSoftwareLicenseRequest, ApiLicensing.SimulateSoftwareLicenseResponse>(
            MethodType.Unary, "acme.licensing.v1" + ".SoftwareLicenseCalculationService", "SimulateSoftwareLicense",
            Wire.Marshaller<ApiLicensing.SimulateSoftwareLicenseRequest>(), Wire.Marshaller<ApiLicensing.SimulateSoftwareLicenseResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.ExplainSlaPolicyDecisionResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.ManagedDevice?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Effective = (request.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                Owner = new ApiIdentity.UserRef
                {
                    Email = (reply3.SoftwareLicense?.Contact?.Email ?? ""),
                    UserId = (reply2.SlaPolicy?.Owner?.UserId ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ExplainSlaPolicyDecision", (request.SlaPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.GetSlaPolicyRulesResponse> GetSlaPolicyRules(ApiHelpdesk.GetSlaPolicyRulesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        _logger.LogInformation("Handling GetSlaPolicyRules for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "GetSlaPolicyRules", "accepted", token);
        var response = new ApiHelpdesk.GetSlaPolicyRulesResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Normalizes a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.NormalizeSlaPolicyResponse> NormalizeSlaPolicy(ApiHelpdesk.NormalizeSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling NormalizeSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.ExplainKnowledgeArticleDecisionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                    {
                        ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                        _ => new ApiCommon.Money()
                    },
                } },
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleValidationService.ExplainKnowledgeArticleDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.NormalizeSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.NormalizeSlaPolicyResponse.Types.Outcome
            {
                NextSteps = { (reply0.Outcome?.NextSteps ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Name = (request.SlaPolicy?.Name ?? ""),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "NormalizeSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Previews a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.PreviewSlaPolicyResponse> PreviewSlaPolicy(ApiHelpdesk.PreviewSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling PreviewSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var profile0 = new MappingProfile<ApiHelpdesk.PreviewSlaPolicyRequest, ApiHelpdesk.ValidateTicketRequest>(value => new ApiHelpdesk.ValidateTicketRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (value.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (value.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                } },
                Customer = new ApiHelpdesk.KnowledgeArticleRef
                {
                    DisplayName = (value.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _helpdeskTicketValidationService.ValidateTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.PreviewTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _helpdeskTicketValidationService.PreviewTicketAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.ScoreKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Ticket?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedAt = (request.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleValidationService.ScoreKnowledgeArticleAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiLicensing.SuggestSoftwareLicensesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiLicensing.SuggestSoftwareLicensesRequest.Types.Filter
            {
                CreatedAfter = (reply2.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply2.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.Ticket?.Assignee?.UserId ?? ""),
                Region = (reply2.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var reply3 = await SendPreviewSlaPolicy3Async(input3, token);
        var response = new ApiHelpdesk.PreviewSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.PreviewSlaPolicyResponse.Types.Outcome
            {
                Message = (reply2.Result?.Message ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply2.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.Ticket?.Audit?.Etag ?? ""),
                },
                Effective = (request.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "PreviewSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Scores a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ScoreSlaPolicyResponse> ScoreSlaPolicy(ApiHelpdesk.ScoreSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ScoreSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.NormalizeTicketRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (request.SlaPolicy?.Owner?.UserId ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.SlaPolicy?.Audit?.CreatedBy ?? ""),
                },
                Comments = { new ApiHelpdesk.TicketComment
                {
                    CreatedAt = (request.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                } },
            },
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _helpdeskTicketValidationService.NormalizeTicketAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiHelpdesk.ScoreSlaPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Ticket?.Audit?.UpdatedBy ?? ""),
                },
                Owner = (request.SlaPolicy?.Owner ?? new ApiIdentity.UserRef()),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ScoreSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Validates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ValidateSlaPolicyResponse> ValidateSlaPolicy(ApiHelpdesk.ValidateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ValidateSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiAuthorization.BrowseAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAuthorization.BrowseAccessPoliciesRequest.Types.Filter
            {
                CreatedAfter = (request.SlaPolicy?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.SlaPolicy?.SlaPolicyId ?? ""),
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiAuthorization.BrowseAccessPoliciesRequest>();
        using var rpc0 = _authorizationAccessPolicyQueryService.BrowseAccessPoliciesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.ShareDocumentTemplateExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.ShareDocumentTemplateExportRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentTemplateExportService.ShareDocumentTemplateExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.CheckTicketEligibilityRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.Ticket = new ApiHelpdesk.Ticket
        {
            Assignee = new ApiIdentity.UserRef
            {
                UserId = (request.SlaPolicy?.Owner?.UserId ?? ""),
            },
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
            },
            Requester = new ApiCommon.ContactInfo
            {
                Email = (request.Details?.Contact?.Email ?? ""),
            },
        };
        using var rpc2 = _helpdeskTicketValidationService.CheckTicketEligibilityAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply2.Ticket?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply2.Ticket?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply1.DocumentTemplate?.BillingAddress?.CountryCode ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (reply1.DocumentTemplate?.Counterparty?.DisplayName ?? ""),
                },
            },
        };
        using var rpc3 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.ValidateSlaPolicyResponse
        {
            Outcome = new ApiHelpdesk.ValidateSlaPolicyResponse.Types.Outcome
            {
                Status = (reply3.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply1.Outcome?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.Ticket?.Assignee?.DisplayName ?? ""),
                    Email = (reply3.KnowledgeArticle?.Counterparty?.Email ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
                Version = (request.SlaPolicy?.Version ?? 0L),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.Route?.Target ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ValidateSlaPolicy", (request.SlaPolicy?.Route?.Target ?? ""), token);
        return response;
    }
    private async Task<ApiLicensing.SuggestSoftwareLicensesResponse> SendPreviewSlaPolicy3Async(ApiLicensing.SuggestSoftwareLicensesRequest request, CancellationToken token)
    {
        return await _slaPolicyValidationEndpointSendPreviewSlaPolicy3Port.SendAsync(request, token);
    }
}
