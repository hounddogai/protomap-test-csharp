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
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPatients = Acme.Patients.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Onboarding;

/// <summary>
/// Moves an equipment between the states of its lifecycle.
/// </summary>
public sealed partial class EquipmentLifecycleEndpoint : ApiOnboarding.EquipmentLifecycleService.EquipmentLifecycleServiceBase
{
    private readonly ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient _auditAuditEventValidationService;
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient _documentsDocumentFolderNotificationService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly ILogger<EquipmentLifecycleEndpoint> _logger;
    private readonly ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient _onboardingEquipmentReportingService;
    private readonly ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient _onboardingEquipmentValidationService;
    private readonly ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient _onboardingOnboardingPlanQueryService;
    private readonly ApiPatients.PatientCalculationService.PatientCalculationServiceClient _patientsPatientCalculationService;
    private readonly ApiPipelines.PipelineService.PipelineServiceClient _pipelinesPipelineService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient _workplacesVisitorPassApprovalService;

    public EquipmentLifecycleEndpoint(
        ApiAudit.AuditEventValidationService.AuditEventValidationServiceClient auditAuditEventValidationService,
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiDocuments.DocumentFolderNotificationService.DocumentFolderNotificationServiceClient documentsDocumentFolderNotificationService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        ILogger<EquipmentLifecycleEndpoint> logger,
        ApiOnboarding.EquipmentReportingService.EquipmentReportingServiceClient onboardingEquipmentReportingService,
        ApiOnboarding.EquipmentValidationService.EquipmentValidationServiceClient onboardingEquipmentValidationService,
        ApiOnboarding.OnboardingPlanQueryService.OnboardingPlanQueryServiceClient onboardingOnboardingPlanQueryService,
        ApiPatients.PatientCalculationService.PatientCalculationServiceClient patientsPatientCalculationService,
        ApiPipelines.PipelineService.PipelineServiceClient pipelinesPipelineService,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiWorkplaces.VisitorPassApprovalService.VisitorPassApprovalServiceClient workplacesVisitorPassApprovalService)
    {
        _auditAuditEventValidationService = auditAuditEventValidationService;
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _documentsDocumentFolderNotificationService = documentsDocumentFolderNotificationService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _logger = logger;
        _onboardingEquipmentReportingService = onboardingEquipmentReportingService;
        _onboardingEquipmentValidationService = onboardingEquipmentValidationService;
        _onboardingOnboardingPlanQueryService = onboardingOnboardingPlanQueryService;
        _patientsPatientCalculationService = patientsPatientCalculationService;
        _pipelinesPipelineService = pipelinesPipelineService;
        _registry = registry;
        _repository = repository;
        _workplacesVisitorPassApprovalService = workplacesVisitorPassApprovalService;
    }
}
