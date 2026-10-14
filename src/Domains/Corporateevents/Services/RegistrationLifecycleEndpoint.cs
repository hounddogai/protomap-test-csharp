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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Corporateevents;

/// <summary>
/// Moves a registration between the states of its lifecycle.
/// </summary>
public sealed partial class RegistrationLifecycleEndpoint : ApiCorporateevents.RegistrationLifecycleService.RegistrationLifecycleServiceBase
{
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient _corporateeventsCorporateEventQueryService;
    private readonly ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient _corporateeventsCorporateEventReportingService;
    private readonly ApiCorporateevents.CorporateEventService.CorporateEventServiceClient _corporateeventsCorporateEventService;
    private readonly ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient _corporateeventsCorporateEventValidationService;
    private readonly ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient _corporateeventsRegistrationQueryService;
    private readonly ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient _corporateeventsRegistrationReportingService;
    private readonly ApiCorporateevents.VenueReportingService.VenueReportingServiceClient _corporateeventsVenueReportingService;
    private readonly ApiCorporateevents.VenueSyncService.VenueSyncServiceClient _corporateeventsVenueSyncService;
    private readonly CallInvoker _invoker;
    private readonly ApiItsm.ProblemSyncService.ProblemSyncServiceClient _itsmProblemSyncService;
    private readonly ILogger<RegistrationLifecycleEndpoint> _logger;
    private readonly ApiPatients.PatientQueryService.PatientQueryServiceClient _patientsPatientQueryService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiPatients.FindPatientsRequest, CallOptions, AsyncUnaryCall<ApiPatients.FindPatientsResponse>> _sendSuspendRegistration0;

    public RegistrationLifecycleEndpoint(
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.CorporateEventQueryService.CorporateEventQueryServiceClient corporateeventsCorporateEventQueryService,
        ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient corporateeventsCorporateEventReportingService,
        ApiCorporateevents.CorporateEventService.CorporateEventServiceClient corporateeventsCorporateEventService,
        ApiCorporateevents.CorporateEventValidationService.CorporateEventValidationServiceClient corporateeventsCorporateEventValidationService,
        ApiCorporateevents.RegistrationQueryService.RegistrationQueryServiceClient corporateeventsRegistrationQueryService,
        ApiCorporateevents.RegistrationReportingService.RegistrationReportingServiceClient corporateeventsRegistrationReportingService,
        ApiCorporateevents.VenueReportingService.VenueReportingServiceClient corporateeventsVenueReportingService,
        ApiCorporateevents.VenueSyncService.VenueSyncServiceClient corporateeventsVenueSyncService,
        CallInvoker invoker,
        ApiItsm.ProblemSyncService.ProblemSyncServiceClient itsmProblemSyncService,
        ILogger<RegistrationLifecycleEndpoint> logger,
        ApiPatients.PatientQueryService.PatientQueryServiceClient patientsPatientQueryService,
        ClientRegistry registry,
        IRecordRepository repository)
    {
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsCorporateEventQueryService = corporateeventsCorporateEventQueryService;
        _corporateeventsCorporateEventReportingService = corporateeventsCorporateEventReportingService;
        _corporateeventsCorporateEventService = corporateeventsCorporateEventService;
        _corporateeventsCorporateEventValidationService = corporateeventsCorporateEventValidationService;
        _corporateeventsRegistrationQueryService = corporateeventsRegistrationQueryService;
        _corporateeventsRegistrationReportingService = corporateeventsRegistrationReportingService;
        _corporateeventsVenueReportingService = corporateeventsVenueReportingService;
        _corporateeventsVenueSyncService = corporateeventsVenueSyncService;
        _invoker = invoker;
        _itsmProblemSyncService = itsmProblemSyncService;
        _logger = logger;
        _patientsPatientQueryService = patientsPatientQueryService;
        _registry = registry;
        _repository = repository;
        _sendSuspendRegistration0 = _patientsPatientQueryService.FindPatientsAsync;
    }
}
