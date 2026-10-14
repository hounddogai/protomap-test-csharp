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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Itsm;

/// <summary>
/// Answers read-only questions about incidents, for screens and reports.
/// </summary>
public sealed partial class IncidentQueryEndpoint : ApiItsm.IncidentQueryService.IncidentQueryServiceBase
{
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiItsm.IncidentValidationService.IncidentValidationServiceClient _itsmIncidentValidationService;
    private readonly ApiItsm.ProblemValidationService.ProblemValidationServiceClient _itsmProblemValidationService;
    private readonly ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient _itsmServiceAssetValidationService;
    private readonly ILogger<IncidentQueryEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public IncidentQueryEndpoint(
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiItsm.IncidentValidationService.IncidentValidationServiceClient itsmIncidentValidationService,
        ApiItsm.ProblemValidationService.ProblemValidationServiceClient itsmProblemValidationService,
        ApiItsm.ServiceAssetValidationService.ServiceAssetValidationServiceClient itsmServiceAssetValidationService,
        ILogger<IncidentQueryEndpoint> logger,
        IRecordRepository repository,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _itsmIncidentValidationService = itsmIncidentValidationService;
        _itsmProblemValidationService = itsmProblemValidationService;
        _itsmServiceAssetValidationService = itsmServiceAssetValidationService;
        _logger = logger;
        _repository = repository;
        _venuesVenueQueryService = venuesVenueQueryService;
    }
}
