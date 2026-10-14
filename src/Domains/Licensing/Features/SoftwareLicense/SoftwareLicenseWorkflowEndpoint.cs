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
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Runs the long-lived workflow that takes a software license from start to finish.
/// </summary>
public sealed partial class SoftwareLicenseWorkflowEndpoint : ApiLicensing.SoftwareLicenseWorkflowService.SoftwareLicenseWorkflowServiceBase
{
    private readonly ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient _documentsDocumentFolderValidationService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient _licensingEntitlementApprovalService;
    private readonly ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient _licensingEntitlementCalculationService;
    private readonly ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient _licensingEntitlementLifecycleService;
    private readonly ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient _licensingSoftwareLicenseApprovalService;
    private readonly ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient _licensingSoftwareLicenseCalculationService;
    private readonly ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient _licensingSoftwareLicenseQueryService;
    private readonly ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient _licensingSoftwareLicenseService;
    private readonly ILogger<SoftwareLicenseWorkflowEndpoint> _logger;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient _telematicsTelemetryReadingQueryService;

    public SoftwareLicenseWorkflowEndpoint(
        ApiDocuments.DocumentFolderValidationService.DocumentFolderValidationServiceClient documentsDocumentFolderValidationService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        CallInvoker invoker,
        ApiLicensing.EntitlementApprovalService.EntitlementApprovalServiceClient licensingEntitlementApprovalService,
        ApiLicensing.EntitlementCalculationService.EntitlementCalculationServiceClient licensingEntitlementCalculationService,
        ApiLicensing.EntitlementLifecycleService.EntitlementLifecycleServiceClient licensingEntitlementLifecycleService,
        ApiLicensing.SoftwareLicenseApprovalService.SoftwareLicenseApprovalServiceClient licensingSoftwareLicenseApprovalService,
        ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceClient licensingSoftwareLicenseCalculationService,
        ApiLicensing.SoftwareLicenseQueryService.SoftwareLicenseQueryServiceClient licensingSoftwareLicenseQueryService,
        ApiLicensing.SoftwareLicenseService.SoftwareLicenseServiceClient licensingSoftwareLicenseService,
        ILogger<SoftwareLicenseWorkflowEndpoint> logger,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiTelematics.TelemetryReadingQueryService.TelemetryReadingQueryServiceClient telematicsTelemetryReadingQueryService)
    {
        _documentsDocumentFolderValidationService = documentsDocumentFolderValidationService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _invoker = invoker;
        _licensingEntitlementApprovalService = licensingEntitlementApprovalService;
        _licensingEntitlementCalculationService = licensingEntitlementCalculationService;
        _licensingEntitlementLifecycleService = licensingEntitlementLifecycleService;
        _licensingSoftwareLicenseApprovalService = licensingSoftwareLicenseApprovalService;
        _licensingSoftwareLicenseCalculationService = licensingSoftwareLicenseCalculationService;
        _licensingSoftwareLicenseQueryService = licensingSoftwareLicenseQueryService;
        _licensingSoftwareLicenseService = licensingSoftwareLicenseService;
        _logger = logger;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _policy = policy;
        _repository = repository;
        _telematicsTelemetryReadingQueryService = telematicsTelemetryReadingQueryService;
    }
}
