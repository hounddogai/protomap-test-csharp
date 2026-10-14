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
using ApiVenues = Acme.Venues.V1;

namespace Acme.Workforce.Documents;

/// <summary>
/// Exports document templates to files and other systems.
/// </summary>
public sealed partial class DocumentTemplateExportEndpoint : ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceBase
{
    private readonly ILogger<DocumentTemplateExportEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public DocumentTemplateExportEndpoint(
        ILogger<DocumentTemplateExportEndpoint> logger,
        IRecordRepository repository,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _logger = logger;
        _repository = repository;
        _venuesVenueQueryService = venuesVenueQueryService;
    }
}
