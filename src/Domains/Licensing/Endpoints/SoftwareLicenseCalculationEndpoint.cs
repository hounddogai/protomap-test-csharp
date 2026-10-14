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
using ApiCareplans = Acme.Careplans.V1;
using ApiCommon = Acme.Common.V1;
using ApiContent = Acme.Content.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLabeling = Acme.Labeling.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPipelines = Acme.Pipelines.V1;
using ApiSearch = Acme.Search.V1;
using ApiSeating = Acme.Seating.V1;

namespace Acme.Workforce.Licensing;

/// <summary>
/// Computes the figures of a software license, such as totals, taxes, and estimates.
/// </summary>
public sealed class SoftwareLicenseCalculationEndpoint : ApiLicensing.SoftwareLicenseCalculationService.SoftwareLicenseCalculationServiceBase
{
    private readonly ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient _careplansCarePlanQueryService;
    private readonly ApiContent.ArticleService.ArticleServiceClient _contentArticleService;
    private readonly ApiDocuments.DocumentExportService.DocumentExportServiceClient _documentsDocumentExportService;
    private readonly ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient _documentsDocumentFolderSyncService;
    private readonly ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient _documentsDocumentNotificationService;
    private readonly ApiDocuments.DocumentQueryService.DocumentQueryServiceClient _documentsDocumentQueryService;
    private readonly ApiDocuments.DocumentValidationService.DocumentValidationServiceClient _documentsDocumentValidationService;
    private readonly CallInvoker _invoker;
    private readonly ApiLabeling.LabelingTaskService.LabelingTaskServiceClient _labelingLabelingTaskService;
    private readonly ILogger<SoftwareLicenseCalculationEndpoint> _logger;
    private readonly ApiPipelines.PipelineService.PipelineServiceClient _pipelinesPipelineService;
    private readonly IRecordRepository _repository;
    private readonly ApiSearch.SearchIndexService.SearchIndexServiceClient _searchSearchIndexService;
    private readonly ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient _seatingSeatMapQueryService;

    public SoftwareLicenseCalculationEndpoint(
        ApiCareplans.CarePlanQueryService.CarePlanQueryServiceClient careplansCarePlanQueryService,
        ApiContent.ArticleService.ArticleServiceClient contentArticleService,
        ApiDocuments.DocumentExportService.DocumentExportServiceClient documentsDocumentExportService,
        ApiDocuments.DocumentFolderSyncService.DocumentFolderSyncServiceClient documentsDocumentFolderSyncService,
        ApiDocuments.DocumentNotificationService.DocumentNotificationServiceClient documentsDocumentNotificationService,
        ApiDocuments.DocumentQueryService.DocumentQueryServiceClient documentsDocumentQueryService,
        ApiDocuments.DocumentValidationService.DocumentValidationServiceClient documentsDocumentValidationService,
        CallInvoker invoker,
        ApiLabeling.LabelingTaskService.LabelingTaskServiceClient labelingLabelingTaskService,
        ILogger<SoftwareLicenseCalculationEndpoint> logger,
        ApiPipelines.PipelineService.PipelineServiceClient pipelinesPipelineService,
        IRecordRepository repository,
        ApiSearch.SearchIndexService.SearchIndexServiceClient searchSearchIndexService,
        ApiSeating.SeatMapQueryService.SeatMapQueryServiceClient seatingSeatMapQueryService)
    {
        _careplansCarePlanQueryService = careplansCarePlanQueryService;
        _contentArticleService = contentArticleService;
        _documentsDocumentExportService = documentsDocumentExportService;
        _documentsDocumentFolderSyncService = documentsDocumentFolderSyncService;
        _documentsDocumentNotificationService = documentsDocumentNotificationService;
        _documentsDocumentQueryService = documentsDocumentQueryService;
        _documentsDocumentValidationService = documentsDocumentValidationService;
        _invoker = invoker;
        _labelingLabelingTaskService = labelingLabelingTaskService;
        _logger = logger;
        _pipelinesPipelineService = pipelinesPipelineService;
        _repository = repository;
        _searchSearchIndexService = searchSearchIndexService;
        _seatingSeatMapQueryService = seatingSeatMapQueryService;
    }
    /// <summary>
    /// Calculates a software license.
    /// </summary>
    public override async Task<ApiLicensing.CalculateSoftwareLicenseResponse> CalculateSoftwareLicense(ApiLicensing.CalculateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CalculateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ExportDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDocuments.ExportDocumentsRequest.Types.Filter
            {
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var stream0 = _documentsDocumentQueryService.ExportDocuments(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received ExportDocuments frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiCareplans.SummarizeCarePlansRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiCareplans.SummarizeCarePlansRequest.Types.Details
            {
                Filters = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _careplansCarePlanQueryService.SummarizeCarePlansAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CalculateSoftwareLicenseResponse
        {
            Breakdown = new ApiLicensing.CalculateSoftwareLicenseResponse.Types.Breakdown
            {
                CurrencyCode = (request.Details?.CurrencyCode ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply1.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CalculateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Compares a software license.
    /// </summary>
    public override async Task<ApiLicensing.CompareSoftwareLicenseOptionsResponse> CompareSoftwareLicenseOptions(ApiLicensing.CompareSoftwareLicenseOptionsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CompareSoftwareLicenseOptions for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.PushDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.PushDocumentFolderRequest.Types.Details
            {
                Payload = ByteString.CopyFromUtf8(JsonFormatter.Default.Format(JsonParser.Default.Parse<ApiCommon.RequestContext>(JsonFormatter.Default.Format((request.Context ?? new ApiCommon.RequestContext()))))),
            },
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (request.Details?.Destination?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                        },
                    } },
                } },
            },
        };
        using var rpc0 = _documentsDocumentFolderSyncService.PushDocumentFolderAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.SendDocumentTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.SoftwareLicense?.Contact?.Email ?? ""),
                    Name = (request.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                    PreferredChannel = (request.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsDocumentNotificationService.SendDocumentTestNoticeAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.CompareSoftwareLicenseOptionsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.Document?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.DocumentFolder?.Manager?.Email ?? ""),
                },
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
                Status = (request.SoftwareLicense?.Status ?? (ApiLicensing.SoftwareLicenseStatus)0),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "CompareSoftwareLicenseOptions", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Estimates a software license.
    /// </summary>
    public override async Task<ApiLicensing.EstimateSoftwareLicenseResponse> EstimateSoftwareLicense(ApiLicensing.EstimateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling EstimateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var batch0 = (request.SoftwareLicense?.Limits ?? new Google.Protobuf.Collections.RepeatedField<ApiLicensing.SoftwareLicenseLimit>()).Select(async item =>
        {
            var input0 = new ApiDocuments.LintDocumentsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Document = new ApiDocuments.Document
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        CountryCode = (request.Details?.Destination?.CountryCode ?? ""),
                    },
                    Counterparty = new ApiDocuments.DocumentFolderRef
                    {
                        DisplayName = (request.SoftwareLicense?.Holder?.DisplayName ?? ""),
                    },
                    Lines = { new ApiDocuments.DocumentLine
                    {
                        Amount = (item.Amount ?? new ApiCommon.Money()),
                    } },
                },
            };
            using var rpc0 = _documentsDocumentValidationService.LintDocumentsAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            return reply0;
        }).ToArray();
        var replies0 = await Task.WhenAll(batch0);
        var reply0 = replies0.LastOrDefault() ?? new ApiDocuments.LintDocumentsResponse();
        var input1 = new ApiDocuments.ScheduleDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentExportService.ScheduleDocumentExportAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        var input2 = new ApiCareplans.CountCarePlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiCareplans.CountCarePlansRequest.Types.Filter
            {
                CreatedAfter = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (reply0.Document?.Counterparty?.DocumentFolderId ?? ""),
                Region = (reply0.Document?.BillingAddress?.Region ?? ""),
            },
        };
        using var rpc2 = _careplansCarePlanQueryService.CountCarePlansAsync(input2, cancellationToken: token);
        var pending2 = rpc2.ResponseAsync;
        await Task.WhenAll(pending1, pending2);
        var reply1 = await pending1;
        var reply2 = await pending2;
        var input3 = new ApiCareplans.ResolveCarePlansRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var descriptor3 = new Method<ApiCareplans.ResolveCarePlansRequest, ApiCareplans.ResolveCarePlansResponse>(
            MethodType.Unary, "acme.careplans.v1" + ".CarePlanQueryService", "ResolveCarePlans",
            Wire.Marshaller<ApiCareplans.ResolveCarePlansRequest>(), Wire.Marshaller<ApiCareplans.ResolveCarePlansResponse>());
        using var rpc3 = _invoker.AsyncUnaryCall(descriptor3, null,
            new CallOptions(cancellationToken: token), input3);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.EstimateSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
                Message = (reply1.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Balance = (request.SoftwareLicense?.Balance ?? new ApiCommon.Money()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "EstimateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Explains a software license.
    /// </summary>
    public override async Task<ApiLicensing.ExplainSoftwareLicenseCalculationResponse> ExplainSoftwareLicenseCalculation(ApiLicensing.ExplainSoftwareLicenseCalculationRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ExplainSoftwareLicenseCalculation for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.SendDocumentTestNoticeRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SendDocumentTestNoticeRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Details?.Contact?.Email ?? ""),
                    Name = (request.Details?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Details?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Details?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc0 = _documentsDocumentNotificationService.SendDocumentTestNoticeAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.ExplainSoftwareLicenseCalculationResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedBy = (reply0.Document?.Audit?.UpdatedBy ?? ""),
                },
                Contact = (request.Details?.Contact ?? new ApiCommon.ContactInfo()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "ExplainSoftwareLicenseCalculation", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a software license.
    /// </summary>
    public override async Task<ApiLicensing.GetSoftwareLicenseRatesResponse> GetSoftwareLicenseRates(ApiLicensing.GetSoftwareLicenseRatesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.SoftwareLicenseId, "software_license_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetSoftwareLicenseRates for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "software_license", request.SoftwareLicenseId,
            "GetSoftwareLicenseRates", "accepted", token);
        var profile0 = new MappingProfile<ApiLicensing.GetSoftwareLicenseRatesRequest, ApiSearch.SearchSearchIndexesRequest>(value => new ApiSearch.SearchSearchIndexesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSearch.SearchSearchIndexesRequest.Types.Filter
            {
                CreatedAfter = (value.Context?.Deadline ?? new Timestamp()),
                OwnerId = (value.SoftwareLicenseId ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _searchSearchIndexService.SearchSearchIndexesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPipelines.CreatePipelineRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Pipeline = new ApiPipelines.Pipeline
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
                Participants = { new ApiPipelines.PipelineParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        input1 = PropertyMap.Copy(input1);
        using var rpc1 = _pipelinesPipelineService.CreatePipelineAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.GetSoftwareLicenseRatesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply1.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Quotes a software license.
    /// </summary>
    public override async Task<ApiLicensing.QuoteSoftwareLicenseResponse> QuoteSoftwareLicense(ApiLicensing.QuoteSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling QuoteSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = request.QuoteSoftwareLicenseToResolveSeatMaps0();
        using var rpc0 = _seatingSeatMapQueryService.ResolveSeatMapsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLicensing.QuoteSoftwareLicenseResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Balance = (request.SoftwareLicense?.Balance ?? new ApiCommon.Money()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "QuoteSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Recalculates a software license.
    /// </summary>
    public override async Task<ApiLicensing.RecalculateSoftwareLicensesResponse> RecalculateSoftwareLicenses(ApiLicensing.RecalculateSoftwareLicensesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RecalculateSoftwareLicenses for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiContent.WatchArticlesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Name = (request.SoftwareLicense?.Contact?.Name ?? new ApiCommon.PersonName()),
                Phone = (request.SoftwareLicense?.Contact?.Phone ?? ""),
            },
        };
        using var stream0 = _contentArticleService.WatchArticles(input0, cancellationToken: token);
        await foreach (var change0 in stream0.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchArticles frame {Type}", change0.GetType().Name);
        }
        var input1 = new ApiDocuments.NotifyDocumentWatchersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.NotifyDocumentWatchersRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    PreferredChannel = (request.SoftwareLicense?.Contact?.PreferredChannel ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        using var rpc1 = _documentsDocumentNotificationService.NotifyDocumentWatchersAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLicensing.RecalculateSoftwareLicensesResponse
        {
            Breakdown = new ApiLicensing.RecalculateSoftwareLicensesResponse.Types.Breakdown
            {
                Total = (reply1.Document?.Total ?? new ApiCommon.Money()),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                Available = (request.SoftwareLicense?.Available ?? new ApiCommon.Money()),
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "RecalculateSoftwareLicenses", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Simulates a software license.
    /// </summary>
    public override async Task<ApiLicensing.SimulateSoftwareLicenseResponse> SimulateSoftwareLicense(ApiLicensing.SimulateSoftwareLicenseRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SimulateSoftwareLicense for {Tenant}", requestContext.TenantId);
        if (request.SoftwareLicense is { } record)
        {
            var snapshot = SoftwareLicenseMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.ResolveDocumentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var packed0 = Any.Pack(input0);
        input0 = packed0.Unpack<ApiDocuments.ResolveDocumentsRequest>();
        using var rpc0 = _documentsDocumentQueryService.ResolveDocumentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiDocuments.CheckDocumentEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.CheckDocumentEligibilityRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.SoftwareLicense?.Contact?.Email ?? ""),
                },
            },
            Document = new ApiDocuments.Document
            {
                Card = new ApiDocuments.DocumentCardSettlement
                {
                    Network = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (request.SoftwareLicense?.Card?.Network ?? ""),
                        _ => ""
                    },
                    Token = (request.SoftwareLicense ?? new ApiLicensing.SoftwareLicense()).PaymentCase switch
                    {
                        ApiLicensing.SoftwareLicense.FundingOneofCase.Card => (request.SoftwareLicense?.Card?.Token ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc1 = _documentsDocumentValidationService.CheckDocumentEligibilityAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiLabeling.CloneLabelingTaskRequest();
        input2.Context = (request.Context ?? new ApiCommon.RequestContext());
        input2.LabelingTask = new ApiLabeling.LabelingTask
        {
            Audit = new ApiCommon.AuditInfo
            {
                UpdatedBy = (request.SoftwareLicense?.Audit?.UpdatedBy ?? ""),
            },
            Customer = new ApiLabeling.LabelSetRef
            {
                Path = (reply1.Document?.Counterparty?.Path ?? ""),
            },
        };
        using var rpc2 = _labelingLabelingTaskService.CloneLabelingTaskAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var input3 = new ApiSeating.FindSeatMapsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiSeating.FindSeatMapsRequest.Types.Filter
            {
                Labels = { (request.SoftwareLicense?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Region = (request.Details?.Destination?.Region ?? ""),
            },
        };
        using var rpc3 = _seatingSeatMapQueryService.FindSeatMapsAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiLicensing.SimulateSoftwareLicenseResponse
        {
            Breakdown = new ApiLicensing.SimulateSoftwareLicenseResponse.Types.Breakdown
            {
                CurrencyCode = (request.SoftwareLicense?.CurrencyCode ?? ""),
                Total = (reply1.Document?.Total ?? new ApiCommon.Money()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Message = (reply2.Result?.Message ?? ""),
                ReferenceIds = { (reply3.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
            SoftwareLicense = new ApiLicensing.SoftwareLicense
            {
                SoftwareLicenseId = (request.SoftwareLicenseId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.SoftwareLicense?.SoftwareLicenseId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "software_license",
            "audit", "SimulateSoftwareLicense", (request.SoftwareLicense?.SoftwareLicenseId ?? ""), token);
        return response;
    }
}
