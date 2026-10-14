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
using ApiDispatch = Acme.Dispatch.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLearning = Acme.Learning.V1;
using ApiTravel = Acme.Travel.V1;

namespace Acme.Workforce.Learning;

public sealed partial class LearningPathQueryEndpoint
{
    /// <summary>
    /// Lists learning paths a page at a time.
    /// </summary>
    public override async Task<ApiLearning.BrowseLearningPathsResponse> BrowseLearningPaths(ApiLearning.BrowseLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling BrowseLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "BrowseLearningPaths", "accepted", token);
        var response = new ApiLearning.BrowseLearningPathsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "BrowseLearningPaths", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams every matching learning path.
    /// </summary>
    public override async Task ExportLearningPaths(ApiLearning.ExportLearningPathsRequest request, IServerStreamWriter<ApiLearning.ExportLearningPathsResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ExportLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "ExportLearningPaths", "accepted", token);
        var input0 = new ApiDispatch.ResolveDispatchOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        using var rpc0 = _dispatchDispatchOrderQueryService.ResolveDispatchOrdersAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiTravel.SummarizeTravelBookingsRequest
        {
            Annotations = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiTravel.SummarizeTravelBookingsRequest.Types.Details
            {
                Filters = { (request.Filter?.Labels ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Format = (request.Format ?? ""),
            },
        };
        var reply1 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _travelTravelBookingQueryService.SummarizeTravelBookingsAsync(input1, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input2 = new ApiLearning.LintCoursesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Course = new ApiLearning.Course
            {
                Address = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Details = new ApiLearning.LintCoursesRequest.Types.Details
            {
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        var reply2 = await _policy.ExecuteAsync(async cancellation =>
        {
            using var rpc = _learningCourseValidationService.LintCoursesAsync(input2, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input3 = new ApiDispatch.SuggestDispatchOrdersRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiDispatch.SuggestDispatchOrdersRequest.Types.Filter
            {
                Query = (request.Filter?.Query ?? ""),
            },
        };
        var reply3 = await Workflow.SendAsync(input3, token, _dispatchDispatchOrderQueryService.SuggestDispatchOrdersAsync);
        var response = new ApiLearning.ExportLearningPathsResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Location = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply2.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Filter?.OwnerId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "ExportLearningPaths", (request.Filter?.OwnerId ?? ""), token);
        await responses.WriteAsync(response, token);
    }
    /// <summary>
    /// Lists learning paths a page at a time.
    /// </summary>
    public override async Task<ApiLearning.FindLearningPathsResponse> FindLearningPaths(ApiLearning.FindLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling FindLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "FindLearningPaths", "accepted", token);
        var response = new ApiLearning.FindLearningPathsResponse();
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "FindLearningPaths", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a learning path.
    /// </summary>
    public override async Task<ApiLearning.LookupLearningPathResponse> LookupLearningPath(ApiLearning.LookupLearningPathRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.LearningPathId, "learning_path_id");
        _logger.LogInformation("Handling LookupLearningPath for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", request.LearningPathId,
            "LookupLearningPath", "accepted", token);
        var input0 = new ApiLearning.ValidateCertificationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.ValidateCertificationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
                Locale = (request.Context?.Locale ?? ""),
            },
        };
        using var rpc0 = _learningCertificationValidationService.ValidateCertificationAsync(input0, cancellationToken: token);
        var pending0 = rpc0.ResponseAsync;
        var input1 = new ApiLearning.GetLearningPathRatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPathId = (request.LearningPathId ?? ""),
        };
        using var rpc1 = _learningLearningPathCalculationService.GetLearningPathRatesAsync(input1, cancellationToken: token);
        var pending1 = rpc1.ResponseAsync;
        await Task.WhenAll(pending0, pending1);
        var reply0 = await pending0;
        var reply1 = await pending1;
        var response = new ApiLearning.LookupLearningPathResponse
        {
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (reply1.LearningPath?.Location?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Locality = (reply1.LearningPath?.Location?.Locality ?? ""),
                        },
                    } },
                } },
                LearningPathId = (request.LearningPathId ?? ""),
                Location = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.Certification?.BillingAddress?.Locality ?? ""),
                },
            },
        };
        return response;
    }
    /// <summary>
    /// Returns several learning paths at once.
    /// </summary>
    public override async Task<ApiLearning.ResolveLearningPathsResponse> ResolveLearningPaths(ApiLearning.ResolveLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling ResolveLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "ResolveLearningPaths", "accepted", token);
        var input0 = new ApiLearning.SimulateLearningPathRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            LearningPath = new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Children = { new ApiLearning.LearningPath
                    {
                        Manager = new ApiIdentity.UserRef
                        {
                            UserId = (request.Context?.Actor?.UserId ?? ""),
                        },
                    } },
                    Manager = new ApiIdentity.UserRef
                    {
                        Email = (request.Context?.Actor?.Email ?? ""),
                    },
                } },
            },
        };
        var descriptor0 = new Method<ApiLearning.SimulateLearningPathRequest, ApiLearning.SimulateLearningPathResponse>(
            MethodType.Unary, "acme.learning.v1" + ".LearningPathCalculationService", "SimulateLearningPath",
            Wire.Marshaller<ApiLearning.SimulateLearningPathRequest>(), Wire.Marshaller<ApiLearning.SimulateLearningPathResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiLearning.SimulateCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.LearningPath?.Audit?.CreatedAt ?? new Timestamp()),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var command1 = new DispatchCommand<ApiLearning.SimulateCertificationRequest>(input1,
            "primary", requestContext.TenantId);
        var routed1 = command1 with { Region = _settings.Value.Region };
        using var rpc1 = _learningCertificationCalculationService.SimulateCertificationAsync(routed1.Payload, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        await _bus.PublishAsync(new TransportNotification<ApiLearning.SimulateCertificationRequest>("acme.learning.v1.LearningPathQueryService/ResolveLearningPaths/1", routed1.Payload.Clone()), token);
        var response = new ApiLearning.ResolveLearningPathsResponse
        {
            LearningPaths = { new ApiLearning.LearningPath
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.LearningPath?.Audit?.Etag ?? ""),
                },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists learning paths a page at a time.
    /// </summary>
    public override async Task<ApiLearning.SuggestLearningPathsResponse> SuggestLearningPaths(ApiLearning.SuggestLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SuggestLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", "batch",
            "SuggestLearningPaths", "accepted", token);
        var profile0 = new MappingProfile<ApiLearning.SuggestLearningPathsRequest, ApiLearning.PreviewCertificationRequest>(value => new ApiLearning.PreviewCertificationRequest
        {
            Certification = new ApiLearning.Certification
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (value.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (value.Filter?.Region ?? ""),
                },
            },
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiLearning.PreviewCertificationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (value.Context?.Actor?.Email ?? ""),
                },
                Locale = (value.Context?.Locale ?? ""),
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _learningCertificationValidationService.PreviewCertificationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiLearning.SuggestLearningPathsResponse
        {
            LearningPaths = { new ApiLearning.LearningPath
            {
                Children = { new ApiLearning.LearningPath
                {
                    Location = new ApiCommon.PostalAddress
                    {
                        Recipient = (reply0.Certification?.BillingAddress?.Recipient ?? ""),
                    },
                } },
            } },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "SuggestLearningPaths", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Summarizes a learning path.
    /// </summary>
    public override async Task<ApiLearning.SummarizeLearningPathsResponse> SummarizeLearningPaths(ApiLearning.SummarizeLearningPathsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        _logger.LogInformation("Handling SummarizeLearningPaths for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "learning_path", request.LearningPathId,
            "SummarizeLearningPaths", "accepted", token);
        var input0 = new ApiLearning.CompareCertificationOptionsRequest
        {
            Annotations = { (request.Annotations ?? new Google.Protobuf.Collections.MapField<string, string>()) },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _learningCertificationCalculationService.CompareCertificationOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.SummarizeLearningPathsToCalculateLearningPath1(reply0);
        using var rpc1 = _learningLearningPathCalculationService.CalculateLearningPathAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiLearning.SummarizeLearningPathsResponse
        {
            Period = (request.Details?.Period ?? new ApiCommon.TimeWindow()),
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Message = (reply1.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.LearningPathId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "learning_path",
            "audit", "SummarizeLearningPaths", (request.LearningPathId ?? ""), token);
        return response;
    }
}
