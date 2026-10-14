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
using ApiLearning = Acme.Learning.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Scheduling;

/// <summary>
/// Moves an availability window between the states of its lifecycle.
/// </summary>
public sealed partial class AvailabilityWindowLifecycleEndpoint : ApiScheduling.AvailabilityWindowLifecycleService.AvailabilityWindowLifecycleServiceBase
{
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.SignatureValidationService.SignatureValidationServiceClient _documentsSignatureValidationService;
    private readonly ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient _employeesEmploymentContractValidationService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient _learningCertificationWorkflowService;
    private readonly ILogger<AvailabilityWindowLifecycleEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient _schedulingAvailabilityWindowCalculationService;
    private readonly ApiScheduling.RosterQueryService.RosterQueryServiceClient _schedulingRosterQueryService;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient _travelTravelBookingWorkflowService;

    public AvailabilityWindowLifecycleEndpoint(
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.SignatureValidationService.SignatureValidationServiceClient documentsSignatureValidationService,
        ApiEmployees.EmploymentContractValidationService.EmploymentContractValidationServiceClient employeesEmploymentContractValidationService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiLearning.CertificationWorkflowService.CertificationWorkflowServiceClient learningCertificationWorkflowService,
        ILogger<AvailabilityWindowLifecycleEndpoint> logger,
        IRecordRepository repository,
        ApiScheduling.AvailabilityWindowCalculationService.AvailabilityWindowCalculationServiceClient schedulingAvailabilityWindowCalculationService,
        ApiScheduling.RosterQueryService.RosterQueryServiceClient schedulingRosterQueryService,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        ApiTravel.TravelBookingWorkflowService.TravelBookingWorkflowServiceClient travelTravelBookingWorkflowService)
    {
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsSignatureValidationService = documentsSignatureValidationService;
        _employeesEmploymentContractValidationService = employeesEmploymentContractValidationService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _learningCertificationWorkflowService = learningCertificationWorkflowService;
        _logger = logger;
        _repository = repository;
        _schedulingAvailabilityWindowCalculationService = schedulingAvailabilityWindowCalculationService;
        _schedulingRosterQueryService = schedulingRosterQueryService;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _travelTravelBookingWorkflowService = travelTravelBookingWorkflowService;
    }
}
