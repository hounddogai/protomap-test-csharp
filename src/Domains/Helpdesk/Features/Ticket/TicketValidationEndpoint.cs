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
using ApiEmployees = Acme.Employees.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiKeymanagement = Acme.Keymanagement.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPipelines = Acme.Pipelines.V1;

namespace Acme.Workforce.Helpdesk;

/// <summary>
/// Checks tickets against the rules before anything acts on them.
/// </summary>
public sealed partial class TicketValidationEndpoint : ApiHelpdesk.TicketValidationService.TicketValidationServiceBase
{
    private readonly ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient _featureflagsFeatureFlagQueryService;
    private readonly ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient _helpdeskKnowledgeArticleCalculationService;
    private readonly ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient _helpdeskTicketCalculationService;
    private readonly CallInvoker _invoker;
    private readonly ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient _keymanagementCryptoKeyCalculationService;
    private readonly ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient _ledgerLedgerAccountValidationService;
    private readonly ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient _licensingEntitlementQueryService;
    private readonly ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient _licensingEntitlementValidationService;
    private readonly ILogger<TicketValidationEndpoint> _logger;
    private readonly ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient _pipelinesPipelineCalculationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ITicketValidationEndpointSendCheckTicketEligibility1Port _ticketValidationEndpointSendCheckTicketEligibility1Port;

    public TicketValidationEndpoint(
        ApiFeatureflags.FeatureFlagQueryService.FeatureFlagQueryServiceClient featureflagsFeatureFlagQueryService,
        ApiHelpdesk.KnowledgeArticleCalculationService.KnowledgeArticleCalculationServiceClient helpdeskKnowledgeArticleCalculationService,
        ApiHelpdesk.TicketCalculationService.TicketCalculationServiceClient helpdeskTicketCalculationService,
        CallInvoker invoker,
        ApiKeymanagement.CryptoKeyCalculationService.CryptoKeyCalculationServiceClient keymanagementCryptoKeyCalculationService,
        ApiLedger.LedgerAccountValidationService.LedgerAccountValidationServiceClient ledgerLedgerAccountValidationService,
        ApiLicensing.EntitlementQueryService.EntitlementQueryServiceClient licensingEntitlementQueryService,
        ApiLicensing.EntitlementValidationService.EntitlementValidationServiceClient licensingEntitlementValidationService,
        ILogger<TicketValidationEndpoint> logger,
        ApiPipelines.PipelineCalculationService.PipelineCalculationServiceClient pipelinesPipelineCalculationService,
        RetryPolicy policy,
        IRecordRepository repository,
        ITicketValidationEndpointSendCheckTicketEligibility1Port ticketValidationEndpointSendCheckTicketEligibility1PortAdapter)
    {
        _featureflagsFeatureFlagQueryService = featureflagsFeatureFlagQueryService;
        _helpdeskKnowledgeArticleCalculationService = helpdeskKnowledgeArticleCalculationService;
        _helpdeskTicketCalculationService = helpdeskTicketCalculationService;
        _invoker = invoker;
        _keymanagementCryptoKeyCalculationService = keymanagementCryptoKeyCalculationService;
        _ledgerLedgerAccountValidationService = ledgerLedgerAccountValidationService;
        _licensingEntitlementQueryService = licensingEntitlementQueryService;
        _licensingEntitlementValidationService = licensingEntitlementValidationService;
        _logger = logger;
        _pipelinesPipelineCalculationService = pipelinesPipelineCalculationService;
        _policy = policy;
        _repository = repository;
        _ticketValidationEndpointSendCheckTicketEligibility1Port = ticketValidationEndpointSendCheckTicketEligibility1PortAdapter;
    }
}
