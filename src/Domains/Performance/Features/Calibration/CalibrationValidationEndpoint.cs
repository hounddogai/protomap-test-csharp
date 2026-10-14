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
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Checks calibrations against the rules before anything acts on them.
/// </summary>
public sealed partial class CalibrationValidationEndpoint : ApiPerformance.CalibrationValidationService.CalibrationValidationServiceBase
{
    private readonly ICalibrationValidationEndpointSendCheckCalibrationEligibility1Port _calibrationValidationEndpointSendCheckCalibrationEligibility1Port;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient _helpdeskEscalationValidationService;
    private readonly ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient _itsmChangeRequestCalculationService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ILogger<CalibrationValidationEndpoint> _logger;
    private readonly ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient _performanceCalibrationCalculationService;
    private readonly ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient _performanceFeedbackCalculationService;
    private readonly ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient _performanceFeedbackValidationService;
    private readonly ApiPerformance.GoalValidationService.GoalValidationServiceClient _performanceGoalValidationService;
    private readonly RetryPolicy _policy;
    private readonly IRecordRepository _repository;
    private readonly ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient _workplacesRoomBookingCalculationService;
    private readonly ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient _workplacesVisitorPassCalculationService;

    public CalibrationValidationEndpoint(
        ICalibrationValidationEndpointSendCheckCalibrationEligibility1Port calibrationValidationEndpointSendCheckCalibrationEligibility1PortAdapter,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        ApiHelpdesk.EscalationValidationService.EscalationValidationServiceClient helpdeskEscalationValidationService,
        ApiItsm.ChangeRequestCalculationService.ChangeRequestCalculationServiceClient itsmChangeRequestCalculationService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ILogger<CalibrationValidationEndpoint> logger,
        ApiPerformance.CalibrationCalculationService.CalibrationCalculationServiceClient performanceCalibrationCalculationService,
        ApiPerformance.FeedbackCalculationService.FeedbackCalculationServiceClient performanceFeedbackCalculationService,
        ApiPerformance.FeedbackValidationService.FeedbackValidationServiceClient performanceFeedbackValidationService,
        ApiPerformance.GoalValidationService.GoalValidationServiceClient performanceGoalValidationService,
        RetryPolicy policy,
        IRecordRepository repository,
        ApiWorkplaces.RoomBookingCalculationService.RoomBookingCalculationServiceClient workplacesRoomBookingCalculationService,
        ApiWorkplaces.VisitorPassCalculationService.VisitorPassCalculationServiceClient workplacesVisitorPassCalculationService)
    {
        _calibrationValidationEndpointSendCheckCalibrationEligibility1Port = calibrationValidationEndpointSendCheckCalibrationEligibility1PortAdapter;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _helpdeskEscalationValidationService = helpdeskEscalationValidationService;
        _itsmChangeRequestCalculationService = itsmChangeRequestCalculationService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _logger = logger;
        _performanceCalibrationCalculationService = performanceCalibrationCalculationService;
        _performanceFeedbackCalculationService = performanceFeedbackCalculationService;
        _performanceFeedbackValidationService = performanceFeedbackValidationService;
        _performanceGoalValidationService = performanceGoalValidationService;
        _policy = policy;
        _repository = repository;
        _workplacesRoomBookingCalculationService = workplacesRoomBookingCalculationService;
        _workplacesVisitorPassCalculationService = workplacesVisitorPassCalculationService;
    }
}
