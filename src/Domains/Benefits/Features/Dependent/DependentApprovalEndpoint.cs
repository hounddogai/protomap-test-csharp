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
using ApiDevices = Acme.Devices.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// Routes dependents through approval, with escalation and delegation.
/// </summary>
public sealed partial class DependentApprovalEndpoint : ApiBenefits.DependentApprovalService.DependentApprovalServiceBase
{
    private readonly ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient _benefitsBenefitPlanWorkflowService;
    private readonly ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient _benefitsEnrollmentQueryService;
    private readonly ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient _benefitsLifeEventCalculationService;
    private readonly ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient _benefitsLifeEventValidationService;
    private readonly ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient _devicesComplianceScanWorkflowService;
    private readonly ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient _helpdeskEscalationApprovalService;
    private readonly ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient _itsmServiceAssetWorkflowService;
    private readonly ILogger<DependentApprovalEndpoint> _logger;
    private readonly ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient _recruitingJobOpeningCalculationService;
    private readonly IRecordRepository _repository;
    private readonly IOptions<MeshOptions> _settings;

    public DependentApprovalEndpoint(
        ApiBenefits.BenefitPlanWorkflowService.BenefitPlanWorkflowServiceClient benefitsBenefitPlanWorkflowService,
        ApiBenefits.EnrollmentQueryService.EnrollmentQueryServiceClient benefitsEnrollmentQueryService,
        ApiBenefits.LifeEventCalculationService.LifeEventCalculationServiceClient benefitsLifeEventCalculationService,
        ApiBenefits.LifeEventValidationService.LifeEventValidationServiceClient benefitsLifeEventValidationService,
        ApiDevices.ComplianceScanWorkflowService.ComplianceScanWorkflowServiceClient devicesComplianceScanWorkflowService,
        ApiHelpdesk.EscalationApprovalService.EscalationApprovalServiceClient helpdeskEscalationApprovalService,
        ApiItsm.ServiceAssetWorkflowService.ServiceAssetWorkflowServiceClient itsmServiceAssetWorkflowService,
        ILogger<DependentApprovalEndpoint> logger,
        ApiRecruiting.JobOpeningCalculationService.JobOpeningCalculationServiceClient recruitingJobOpeningCalculationService,
        IRecordRepository repository,
        IOptions<MeshOptions> settings)
    {
        _benefitsBenefitPlanWorkflowService = benefitsBenefitPlanWorkflowService;
        _benefitsEnrollmentQueryService = benefitsEnrollmentQueryService;
        _benefitsLifeEventCalculationService = benefitsLifeEventCalculationService;
        _benefitsLifeEventValidationService = benefitsLifeEventValidationService;
        _devicesComplianceScanWorkflowService = devicesComplianceScanWorkflowService;
        _helpdeskEscalationApprovalService = helpdeskEscalationApprovalService;
        _itsmServiceAssetWorkflowService = itsmServiceAssetWorkflowService;
        _logger = logger;
        _recruitingJobOpeningCalculationService = recruitingJobOpeningCalculationService;
        _repository = repository;
        _settings = settings;
    }
}
