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

/// <summary>
/// Moves an offer between the states of its lifecycle.
/// </summary>
public sealed partial class OfferLifecycleEndpoint : ApiRecruiting.OfferLifecycleService.OfferLifecycleServiceBase
{
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient _fraudRiskAssessmentCalculationService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient _itsmIncidentWorkflowService;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ILogger<OfferLifecycleEndpoint> _logger;
    private readonly IOfferLifecycleEndpointSendTerminateOffer2Port _offerLifecycleEndpointSendTerminateOffer2Port;
    private readonly RetryPolicy _policy;
    private readonly ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient _recruitingCandidateCalculationService;
    private readonly ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient _recruitingCandidateValidationService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public OfferLifecycleEndpoint(
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiFraud.RiskAssessmentCalculationService.RiskAssessmentCalculationServiceClient fraudRiskAssessmentCalculationService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        CallInvoker invoker,
        ApiItsm.IncidentWorkflowService.IncidentWorkflowServiceClient itsmIncidentWorkflowService,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ILogger<OfferLifecycleEndpoint> logger,
        IOfferLifecycleEndpointSendTerminateOffer2Port offerLifecycleEndpointSendTerminateOffer2PortAdapter,
        RetryPolicy policy,
        ApiRecruiting.CandidateCalculationService.CandidateCalculationServiceClient recruitingCandidateCalculationService,
        ApiRecruiting.CandidateValidationService.CandidateValidationServiceClient recruitingCandidateValidationService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _fraudRiskAssessmentCalculationService = fraudRiskAssessmentCalculationService;
        _identityUserValidationService = identityUserValidationService;
        _invoker = invoker;
        _itsmIncidentWorkflowService = itsmIncidentWorkflowService;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _logger = logger;
        _offerLifecycleEndpointSendTerminateOffer2Port = offerLifecycleEndpointSendTerminateOffer2PortAdapter;
        _policy = policy;
        _recruitingCandidateCalculationService = recruitingCandidateCalculationService;
        _recruitingCandidateValidationService = recruitingCandidateValidationService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _repository = repository;
        _settings = settings;
    }
}
