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
using ApiAudiences = Acme.Audiences.V1;
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Helpdesk;

public sealed partial class SlaPolicyCalculationEndpoint
{
    /// <summary>
    /// Calculates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.CalculateSlaPolicyResponse> CalculateSlaPolicy(ApiHelpdesk.CalculateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CalculateSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiIdentity.CheckUserEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            User = new ApiIdentity.User
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiIdentity.CheckUserEligibilityRequest, ApiIdentity.CheckUserEligibilityResponse>(
            MethodType.Unary, "acme.identity.v1" + ".UserValidationService", "CheckUserEligibility",
            Wire.Marshaller<ApiIdentity.CheckUserEligibilityRequest>(), Wire.Marshaller<ApiIdentity.CheckUserEligibilityResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.EstimateKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.User?.PrimaryAddress?.CountryCode ?? ""),
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.User?.Audit?.UpdatedAt ?? new Timestamp()),
                },
            },
        };
        var command1 = new DispatchCommand<ApiHelpdesk.EstimateKnowledgeArticleRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var profile2 = new MappingProfile<ApiHelpdesk.CalculateSlaPolicyRequest, ApiAudiences.BrowseAudiencesRequest>(value => new ApiAudiences.BrowseAudiencesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiAudiences.BrowseAudiencesRequest.Types.Filter
            {
                CreatedAfter = (reply1.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (value.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.User?.RelationshipManager?.UserId ?? ""),
                Region = (reply0.User?.PrimaryAddress?.Region ?? ""),
            },
        });
        var input2 = profile2.Map(request);
        using var rpc2 = _audiencesAudienceQueryService.BrowseAudiencesAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.CalculateSlaPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply2.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                    UpdatedBy = (reply0.User?.Audit?.UpdatedBy ?? ""),
                },
                Description = (request.SlaPolicy?.Description ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (reply0.User?.UserId ?? ""),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "CalculateSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.CompareSlaPolicyOptionsResponse> CompareSlaPolicyOptions(ApiHelpdesk.CompareSlaPolicyOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling CompareSlaPolicyOptions for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.CheckTicketEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Ticket = new ApiHelpdesk.Ticket
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                    UpdatedAt = (request.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _helpdeskTicketValidationService.CheckTicketEligibilityAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.CompareSlaPolicyOptionsToCalculateServiceAsset1(reply0);
        using var rpc1 = _itsmServiceAssetCalculationService.CalculateServiceAssetAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.SimulateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.SlaPolicy?.Audit?.Etag ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                    {
                        Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                        {
                            ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleCalculationService.SimulateKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiDevices.BrowseManagedDevicesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDevices.BrowseManagedDevicesRequest.Types.Filter
            {
                CreatedAfter = (reply2.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply2.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply1.ServiceAsset?.Components?.GetValueOrDefault("primary")?.ComponentId ?? ""),
                Region = (reply2.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var labels3 = input3.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input3.Context.Baggage.Clear();
        input3.Context.Baggage.Add(labels3);
        using var rpc3 = _devicesManagedDeviceQueryService.BrowseManagedDevicesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiHelpdesk.CompareSlaPolicyOptionsResponse
        {
            Breakdown = new ApiHelpdesk.CompareSlaPolicyOptionsResponse.Types.Breakdown
            {
                CurrencyCode = (reply1.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply3.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply1.ServiceAsset?.Audit?.CreatedBy ?? ""),
                },
                Description = (request.SlaPolicy?.Description ?? ""),
                Owner = new ApiIdentity.UserRef
                {
                    DisplayName = (reply2.KnowledgeArticle?.Counterparty?.DisplayName ?? ""),
                },
                Priority = (reply0.Ticket?.Priority ?? 0),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "CompareSlaPolicyOptions", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.EstimateSlaPolicyResponse> EstimateSlaPolicy(ApiHelpdesk.EstimateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling EstimateSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.ExplainLicenseAssignmentDecisionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LicenseAssignment = new ApiLicensing.LicenseAssignment
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (request.SlaPolicy?.Audit?.UpdatedBy ?? ""),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiLicensing.EntitlementRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
            },
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiLicensing.ExplainLicenseAssignmentDecisionRequest>();
        using var rpc0 = _licensingLicenseAssignmentValidationService.ExplainLicenseAssignmentDecisionAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.CalculateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.CalculateKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Recipient = (reply0.LicenseAssignment?.BillingAddress?.Recipient ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Card = new ApiHelpdesk.KnowledgeArticleCardSettlement
                {
                    HolderName = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (reply0.LicenseAssignment?.Card?.HolderName ?? ""),
                        _ => ""
                    },
                    Token = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Card => (reply0.LicenseAssignment?.Card?.Token ?? ""),
                        _ => ""
                    },
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
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.CalculateKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiHelpdesk.EstimateKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                Bank = new ApiHelpdesk.KnowledgeArticleBankSettlement
                {
                    AccountHolder = (reply0.LicenseAssignment ?? new ApiLicensing.LicenseAssignment()).PaymentCase switch
                    {
                        ApiLicensing.LicenseAssignment.SettlementOneofCase.Bank => (reply0.LicenseAssignment?.Bank?.AccountHolder ?? ""),
                        _ => ""
                    },
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (request.Details?.Destination?.PostalCode ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                    {
                        Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                        {
                            ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc2 = _helpdeskKnowledgeArticleCalculationService.EstimateKnowledgeArticleAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiHelpdesk.EstimateSlaPolicyResponse
        {
            Breakdown = new ApiHelpdesk.EstimateSlaPolicyResponse.Types.Breakdown
            {
                CurrencyCode = (reply2.Breakdown?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Outcome?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.LicenseAssignment?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (reply1.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                    UpdatedBy = (reply1.KnowledgeArticle?.Audit?.UpdatedBy ?? ""),
                },
                Effective = (request.SlaPolicy?.Effective ?? new ApiCommon.TimeWindow()),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "EstimateSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.ExplainSlaPolicyCalculationResponse> ExplainSlaPolicyCalculation(ApiHelpdesk.ExplainSlaPolicyCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExplainSlaPolicyCalculation for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiLicensing.LintEntitlementsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLicensing.LintEntitlementsRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
            },
            Entitlement = new ApiLicensing.Entitlement
            {
                Block = new ApiLicensing.EntitlementBlockAction
                {
                    NotifyEmail = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                    {
                        ApiHelpdesk.SlaPolicy.ActionOneofCase.Block => (request.SlaPolicy?.Block?.NotifyEmail ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _licensingEntitlementValidationService.LintEntitlementsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.ExportIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.ExportIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (request.SlaPolicy?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Result?.CorrelationId ?? ""),
            },
        };
        using var stream1 = _itsmIncidentQueryService.ExportIncidents(input1, cancellationToken: token);
        await foreach (var change1 in stream1.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportIncidents frame {Type}", change1.GetType().Name);
        }
        var response = new ApiHelpdesk.ExplainSlaPolicyCalculationResponse
        {
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Entitlement?.Audit?.CreatedAt ?? new Timestamp()),
                },
                Priority = (request.SlaPolicy?.Priority ?? 0),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "ExplainSlaPolicyCalculation", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.GetSlaPolicyRatesResponse> GetSlaPolicyRates(ApiHelpdesk.GetSlaPolicyRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SlaPolicyId, "sla_policy_id");
        _logger.LogInformation("Handling GetSlaPolicyRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy", request.SlaPolicyId,
            "GetSlaPolicyRates", "accepted", token);
        var input0 = new ApiSeating.SummarizeSeatMapsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiSeating.SummarizeSeatMapsRequest.Types.Details
            {
                Filters = { (request.Context?.Baggage ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
        };
        var reply0 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _seatingSeatMapQueryService.SummarizeSeatMapsAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiHelpdesk.GetSlaPolicyRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.QuoteSlaPolicyResponse> QuoteSlaPolicy(ApiHelpdesk.QuoteSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling QuoteSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDevices.CompareComplianceScanOptionsRequest
        {
            ComplianceScan = new ApiDevices.ComplianceScan
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDevices.CompareComplianceScanOptionsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
            Reason = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
            {
                ApiHelpdesk.SlaPolicy.ActionOneofCase.Block => (request.SlaPolicy?.Block?.Reason ?? ""),
                _ => ""
            },
        };
        using var rpc0 = _devicesComplianceScanCalculationService.CompareComplianceScanOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiHelpdesk.QuoteKnowledgeArticleRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiHelpdesk.QuoteKnowledgeArticleRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                },
            },
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Details?.Destination?.Region ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.SlaPolicy?.Owner?.Email ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                    {
                        Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                        {
                            ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc1 = _helpdeskKnowledgeArticleCalculationService.QuoteKnowledgeArticleAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.QuoteSlaPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply1.KnowledgeArticle?.Audit?.CreatedAt ?? new Timestamp()),
                },
                SlaPolicyId = (request.SlaPolicyId ?? ""),
                Status = (request.SlaPolicy?.Status ?? (ApiHelpdesk.SlaPolicyStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "QuoteSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a sla policy.
    /// </summary>
    public override async Task<ApiHelpdesk.SimulateSlaPolicyResponse> SimulateSlaPolicy(ApiHelpdesk.SimulateSlaPolicyRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SimulateSlaPolicy for {Tenant}", requestContext.TenantId);
        if (request.SlaPolicy is { } record)
        {
            var snapshot = SlaPolicyMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiHelpdesk.ExplainKnowledgeArticleCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (request.Details?.Destination?.Locality ?? ""),
                    Recipient = (request.Details?.Destination?.Recipient ?? ""),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.SlaPolicy?.Owner?.DisplayName ?? ""),
                },
                Lines = { new ApiHelpdesk.KnowledgeArticleLine
                {
                    Tax = new ApiHelpdesk.KnowledgeArticleLineTax
                    {
                        Amount = (request.SlaPolicy ?? new ApiHelpdesk.SlaPolicy()).ActionCase switch
                        {
                            ApiHelpdesk.SlaPolicy.ActionOneofCase.Discount => (request.SlaPolicy?.Discount?.Amount ?? new ApiCommon.Money()),
                            _ => new ApiCommon.Money()
                        },
                    },
                } },
            },
        };
        using var rpc0 = _helpdeskKnowledgeArticleCalculationService.ExplainKnowledgeArticleCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiItsm.BrowseIncidentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiItsm.BrowseIncidentsRequest.Types.Filter
            {
                CreatedAfter = (request.SlaPolicy?.Audit?.UpdatedAt ?? new Timestamp()),
                Labels = { (reply0.KnowledgeArticle?.Metadata ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.SlaPolicy?.Owner?.UserId ?? ""),
                Region = (reply0.KnowledgeArticle?.BillingAddress?.Region ?? ""),
            },
        };
        var descriptor1 = new Method<ApiItsm.BrowseIncidentsRequest, ApiItsm.BrowseIncidentsResponse>(
            MethodType.Unary, "acme.itsm.v1" + ".IncidentQueryService", "BrowseIncidents",
            Wire.Marshaller<ApiItsm.BrowseIncidentsRequest>(), Wire.Marshaller<ApiItsm.BrowseIncidentsResponse>());
        using var rpc1 = _invoker.AsyncUnaryCall(descriptor1, null,
            new CallOptions(cancellationToken: token), input1);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiHelpdesk.SimulateSlaPolicyResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
            SlaPolicy = new ApiHelpdesk.SlaPolicy
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply0.KnowledgeArticle?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Priority = (request.SlaPolicy?.Priority ?? 0),
                SlaPolicyId = (request.SlaPolicyId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SlaPolicy?.SlaPolicyId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "sla_policy",
            "audit", "SimulateSlaPolicy", (request.SlaPolicy?.SlaPolicyId ?? ""), token);
        return response;
    }
}
