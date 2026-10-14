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
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiDatasets = Acme.Datasets.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiLineage = Acme.Lineage.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiSearch = Acme.Search.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Answers read-only questions about benefit plans, for screens and reports.
/// </summary>
public sealed partial class BenefitPlanQueryEndpoint : ApiBenefits.BenefitPlanQueryService.BenefitPlanQueryServiceBase
{
    private readonly ApiBenefits.DependentValidationService.DependentValidationServiceClient _benefitsDependentValidationService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly ApiDatasets.DatasetService.DatasetServiceClient _datasetsDatasetService;
    private readonly ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient _documentsDocumentFolderQueryService;
    private readonly ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient _documentsDocumentTemplateHistoryService;
    private readonly ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient _helpdeskKnowledgeArticleValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient _licensingLicenseAssignmentCalculationService;
    private readonly ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient _lineageLineageNodeCalculationService;
    private readonly ILogger<BenefitPlanQueryEndpoint> _logger;
    private readonly ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient _personalizationProfileCalculationService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly IOptions<MeshOptions> _settings;

    public BenefitPlanQueryEndpoint(
        ApiBenefits.DependentValidationService.DependentValidationServiceClient benefitsDependentValidationService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        ApiDatasets.DatasetService.DatasetServiceClient datasetsDatasetService,
        ApiDocuments.DocumentFolderQueryService.DocumentFolderQueryServiceClient documentsDocumentFolderQueryService,
        ApiDocuments.DocumentTemplateHistoryService.DocumentTemplateHistoryServiceClient documentsDocumentTemplateHistoryService,
        ApiHelpdesk.KnowledgeArticleValidationService.KnowledgeArticleValidationServiceClient helpdeskKnowledgeArticleValidationService,
        CallInvoker invoker,
        ApiLicensing.LicenseAssignmentCalculationService.LicenseAssignmentCalculationServiceClient licensingLicenseAssignmentCalculationService,
        ApiLineage.LineageNodeCalculationService.LineageNodeCalculationServiceClient lineageLineageNodeCalculationService,
        ILogger<BenefitPlanQueryEndpoint> logger,
        ApiPersonalization.ProfileCalculationService.ProfileCalculationServiceClient personalizationProfileCalculationService,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        IOptions<MeshOptions> settings)
    {
        _benefitsDependentValidationService = benefitsDependentValidationService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _datasetsDatasetService = datasetsDatasetService;
        _documentsDocumentFolderQueryService = documentsDocumentFolderQueryService;
        _documentsDocumentTemplateHistoryService = documentsDocumentTemplateHistoryService;
        _helpdeskKnowledgeArticleValidationService = helpdeskKnowledgeArticleValidationService;
        _invoker = invoker;
        _licensingLicenseAssignmentCalculationService = licensingLicenseAssignmentCalculationService;
        _lineageLineageNodeCalculationService = lineageLineageNodeCalculationService;
        _logger = logger;
        _personalizationProfileCalculationService = personalizationProfileCalculationService;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _settings = settings;
    }
}
