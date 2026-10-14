using Acme.Workforce.Platform;
using Google.Protobuf;
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
/// Keeps every version of a signature and restores earlier ones.
/// </summary>
public sealed partial class SignatureHistoryEndpoint : ApiDocuments.SignatureHistoryService.SignatureHistoryServiceBase
{
    private readonly ILogger<SignatureHistoryEndpoint> _logger;
    private readonly IRecordRepository _repository;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;

    public SignatureHistoryEndpoint(
        ILogger<SignatureHistoryEndpoint> logger,
        IRecordRepository repository,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService)
    {
        _logger = logger;
        _repository = repository;
        _venuesVenueQueryService = venuesVenueQueryService;
    }
}
