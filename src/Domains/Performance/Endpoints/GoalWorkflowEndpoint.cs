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
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiInsurance = Acme.Insurance.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiShipping = Acme.Shipping.V1;

namespace Acme.Workforce.Performance;

/// <summary>
/// Runs the long-lived workflow that takes a goal from start to finish.
/// </summary>
public sealed class GoalWorkflowEndpoint : ApiPerformance.GoalWorkflowService.GoalWorkflowServiceBase
{
    private readonly ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient _authorizationAccessPolicyValidationService;
    private readonly ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient _corporateeventsRegistrationWorkflowService;
    private readonly ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient _documentsDocumentFolderExportService;
    private readonly ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient _documentsDocumentTemplateExportService;
    private readonly ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient _documentsSignatureCalculationService;
    private readonly IGoalWorkflowEndpointSendListGoalWorkflows0Port _goalWorkflowEndpointSendListGoalWorkflows0Port;
    private readonly ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient _helpdeskEscalationReportingService;
    private readonly ApiIdentity.UserValidationService.UserValidationServiceClient _identityUserValidationService;
    private readonly ApiInsurance.PolicyCalculationService.PolicyCalculationServiceClient _insurancePolicyCalculationService;
    private readonly ILogger<GoalWorkflowEndpoint> _logger;
    private readonly ApiPerformance.GoalApprovalService.GoalApprovalServiceClient _performanceGoalApprovalService;
    private readonly ApiPerformance.GoalQueryService.GoalQueryServiceClient _performanceGoalQueryService;
    private readonly ApiPerformance.ReviewQueryService.ReviewQueryServiceClient _performanceReviewQueryService;
    private readonly ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient _performanceReviewWorkflowService;
    private readonly ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient _recruitingInterviewCalculationService;
    private readonly ApiRecruiting.JobOpeningService.JobOpeningServiceClient _recruitingJobOpeningService;
    private readonly ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient _recruitingOfferCalculationService;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly Func<ApiDocuments.RetryDocumentFolderExportRequest, CallOptions, AsyncUnaryCall<ApiDocuments.RetryDocumentFolderExportResponse>> _sendStartGoalWorkflow1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiShipping.ShipmentService.ShipmentServiceClient _shippingShipmentService;

    public GoalWorkflowEndpoint(
        ApiAuthorization.AccessPolicyValidationService.AccessPolicyValidationServiceClient authorizationAccessPolicyValidationService,
        ApiCorporateevents.RegistrationWorkflowService.RegistrationWorkflowServiceClient corporateeventsRegistrationWorkflowService,
        ApiDocuments.DocumentFolderExportService.DocumentFolderExportServiceClient documentsDocumentFolderExportService,
        ApiDocuments.DocumentTemplateExportService.DocumentTemplateExportServiceClient documentsDocumentTemplateExportService,
        ApiDocuments.SignatureCalculationService.SignatureCalculationServiceClient documentsSignatureCalculationService,
        IGoalWorkflowEndpointSendListGoalWorkflows0Port goalWorkflowEndpointSendListGoalWorkflows0PortAdapter,
        ApiHelpdesk.EscalationReportingService.EscalationReportingServiceClient helpdeskEscalationReportingService,
        ApiIdentity.UserValidationService.UserValidationServiceClient identityUserValidationService,
        ApiInsurance.PolicyCalculationService.PolicyCalculationServiceClient insurancePolicyCalculationService,
        ILogger<GoalWorkflowEndpoint> logger,
        ApiPerformance.GoalApprovalService.GoalApprovalServiceClient performanceGoalApprovalService,
        ApiPerformance.GoalQueryService.GoalQueryServiceClient performanceGoalQueryService,
        ApiPerformance.ReviewQueryService.ReviewQueryServiceClient performanceReviewQueryService,
        ApiPerformance.ReviewWorkflowService.ReviewWorkflowServiceClient performanceReviewWorkflowService,
        ApiRecruiting.InterviewCalculationService.InterviewCalculationServiceClient recruitingInterviewCalculationService,
        ApiRecruiting.JobOpeningService.JobOpeningServiceClient recruitingJobOpeningService,
        ApiRecruiting.OfferCalculationService.OfferCalculationServiceClient recruitingOfferCalculationService,
        ClientRegistry registry,
        IRecordRepository repository,
        IOptions<MeshOptions> settings,
        ApiShipping.ShipmentService.ShipmentServiceClient shippingShipmentService)
    {
        _authorizationAccessPolicyValidationService = authorizationAccessPolicyValidationService;
        _corporateeventsRegistrationWorkflowService = corporateeventsRegistrationWorkflowService;
        _documentsDocumentFolderExportService = documentsDocumentFolderExportService;
        _documentsDocumentTemplateExportService = documentsDocumentTemplateExportService;
        _documentsSignatureCalculationService = documentsSignatureCalculationService;
        _goalWorkflowEndpointSendListGoalWorkflows0Port = goalWorkflowEndpointSendListGoalWorkflows0PortAdapter;
        _helpdeskEscalationReportingService = helpdeskEscalationReportingService;
        _identityUserValidationService = identityUserValidationService;
        _insurancePolicyCalculationService = insurancePolicyCalculationService;
        _logger = logger;
        _performanceGoalApprovalService = performanceGoalApprovalService;
        _performanceGoalQueryService = performanceGoalQueryService;
        _performanceReviewQueryService = performanceReviewQueryService;
        _performanceReviewWorkflowService = performanceReviewWorkflowService;
        _recruitingInterviewCalculationService = recruitingInterviewCalculationService;
        _recruitingJobOpeningService = recruitingJobOpeningService;
        _recruitingOfferCalculationService = recruitingOfferCalculationService;
        _registry = registry;
        _repository = repository;
        _settings = settings;
        _shippingShipmentService = shippingShipmentService;
        _sendStartGoalWorkflow1 = _documentsDocumentFolderExportService.RetryDocumentFolderExportAsync;
    }
    /// <summary>
    /// Advances a goal.
    /// </summary>
    public override async Task<ApiPerformance.AdvanceGoalWorkflowResponse> AdvanceGoalWorkflow(ApiPerformance.AdvanceGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling AdvanceGoalWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var reply0 = new ApiHelpdesk.CountEscalationTotalsResponse();
        foreach (var item in (request.Goal?.Comments ?? new Google.Protobuf.Collections.RepeatedField<ApiPerformance.GoalComment>()).Select(value => new[] { value }).SelectMany(group => group))
        {
            var input0 = new ApiHelpdesk.CountEscalationTotalsRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Filter = new ApiHelpdesk.CountEscalationTotalsRequest.Types.Filter
                {
                    CreatedAfter = (item.CreatedAt ?? new Timestamp()),
                    OwnerId = (item.CommentId ?? ""),
                },
            };
            using var rpc0 = _helpdeskEscalationReportingService.CountEscalationTotalsAsync(input0, cancellationToken: token);
            reply0 = await rpc0.ResponseAsync;
        }
        var input1 = new ApiIdentity.PreviewUserRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiIdentity.PreviewUserRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            User = new ApiIdentity.User
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Phone = (request.Details?.Notify?.Phone ?? ""),
                },
            },
        };
        var reply1 = await Workflow.SendAsync(input1, token, _identityUserValidationService.PreviewUserAsync);
        var input2 = new ApiDocuments.SimulateSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.SimulateSignatureRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Lines = { (reply1.User?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
            Signature = new ApiDocuments.Signature
            {
                CorrelationId = (reply0.Result?.CorrelationId ?? ""),
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (reply1.User?.RelationshipManager?.DisplayName ?? ""),
                    Number = (reply1.User ?? new ApiIdentity.User()).IdentityDocumentCase switch
                    {
                        ApiIdentity.User.IdentityDocumentOneofCase.Passport => (reply1.User?.Passport?.Number ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc2 = _documentsSignatureCalculationService.SimulateSignatureAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiPerformance.AdvanceGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply1.User?.UserId ?? ""),
                },
                GoalId = (request.GoalId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Name = (reply1.User?.Name ?? new ApiCommon.PersonName()),
                },
                Status = (request.Goal?.Status ?? (ApiPerformance.GoalStatus)0),
            },
            Outcome = new ApiPerformance.AdvanceGoalWorkflowResponse.Types.Outcome
            {
                Status = (reply2.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply2.Result?.CorrelationId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "AdvanceGoalWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Cancels a goal.
    /// </summary>
    public override async Task<ApiPerformance.CancelGoalWorkflowResponse> CancelGoalWorkflow(ApiPerformance.CancelGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CancelGoalWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.BrowseReviewsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.BrowseReviewsRequest.Types.Filter
            {
                CreatedAfter = (request.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                Labels = { (request.Details?.Parameters ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                OwnerId = (request.Goal?.Sla?.PolicyId ?? ""),
            },
        };
        var named0 = _registry.Get<ApiPerformance.ReviewQueryService.ReviewQueryServiceClient>("acme.performance.v1.ReviewQueryService");
        using var rpc0 = named0.BrowseReviewsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiRecruiting.CalculateOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (request.Goal?.Audit?.CreatedBy ?? ""),
                    UpdatedAt = (request.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    DisplayName = (request.Goal?.Assignee?.DisplayName ?? ""),
                },
                Number = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                {
                    ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.Number ?? ""),
                    _ => ""
                },
            },
        };
        using var rpc1 = _recruitingOfferCalculationService.CalculateOfferAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiAuthorization.PreviewAccessPolicyRequest
        {
            AccessPolicy = new ApiAuthorization.AccessPolicy
            {
                Owner = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiAuthorization.PreviewAccessPolicyRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Goal?.Assignee?.Email ?? ""),
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
        };
        var command2 = new DispatchCommand<ApiAuthorization.PreviewAccessPolicyRequest>(input2,
            "primary", requestContext.TenantId);
        var routed2 = command2 with { Region = _settings.Value.Region };
        using var rpc2 = _authorizationAccessPolicyValidationService.PreviewAccessPolicyAsync(routed2.Payload, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var profile3 = new MappingProfile<ApiPerformance.CancelGoalWorkflowRequest, ApiInsurance.GetPolicyRatesRequest>(value => new ApiInsurance.GetPolicyRatesRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            PolicyId = (value.Goal?.Sla?.PolicyId ?? ""),
        });
        var input3 = profile3.Map(request);
        using var rpc3 = _insurancePolicyCalculationService.GetPolicyRatesAsync(input3, cancellationToken: token);
        var reply3 = await rpc3.ResponseAsync;
        var response = new ApiPerformance.CancelGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (reply3.Policy?.Contact?.Email ?? ""),
                },
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply1.Offer?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.Offer?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                GoalId = (request.GoalId ?? ""),
                Priority = (request.Goal?.Priority ?? 0),
                Sla = new ApiPerformance.GoalSla
                {
                    PolicyId = (reply3.Policy?.PolicyId ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                ReferenceIds = { (reply2.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "CancelGoalWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns a goal.
    /// </summary>
    public override async Task<ApiPerformance.InspectGoalWorkflowResponse> InspectGoalWorkflow(ApiPerformance.InspectGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.GoalId, "goal_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling InspectGoalWorkflow for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", request.GoalId,
            "InspectGoalWorkflow", "accepted", token);
        var input0 = new ApiRecruiting.CalculateInterviewRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Interview = new ApiRecruiting.Interview
            {
                Organizer = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
        };
        input0 = PropertyMap.Copy(input0);
        using var rpc0 = _recruitingInterviewCalculationService.CalculateInterviewAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = request.InspectGoalWorkflowToRetryDocumentTemplateExport1(reply0);
        using var rpc1 = _documentsDocumentTemplateExportService.RetryDocumentTemplateExportAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.InspectGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (reply0.Interview?.Audit?.CreatedAt ?? new Timestamp()),
                    Etag = (reply1.DocumentTemplate?.Audit?.Etag ?? ""),
                    UpdatedAt = (reply1.DocumentTemplate?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                GoalId = (request.GoalId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists goals a page at a time.
    /// </summary>
    public override async Task<ApiPerformance.ListGoalWorkflowsResponse> ListGoalWorkflows(ApiPerformance.ListGoalWorkflowsRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListGoalWorkflows for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "goal", "batch",
            "ListGoalWorkflows", "accepted", token);
        var input0 = new ApiPerformance.ResolveGoalsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var reply0 = await SendListGoalWorkflows0Async(input0, token);
        var response = new ApiPerformance.ListGoalWorkflowsResponse
        {
            Result = new ApiCommon.OperationResult
            {
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "ListGoalWorkflows", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Pauses a goal.
    /// </summary>
    public override async Task<ApiPerformance.PauseGoalWorkflowResponse> PauseGoalWorkflow(ApiPerformance.PauseGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling PauseGoalWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiShipping.BatchGetShipmentsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
        var labels0 = input0.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        input0.Context.Baggage.Clear();
        input0.Context.Baggage.Add(labels0);
        using var rpc0 = _shippingShipmentService.BatchGetShipmentsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiPerformance.StartReviewWorkflowRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiPerformance.StartReviewWorkflowRequest.Types.Details
            {
                Notify = new ApiCommon.ContactInfo
                {
                    Name = (request.Details?.Notify?.Name ?? new ApiCommon.PersonName()),
                },
            },
            Review = new ApiPerformance.Review
            {
                Counterparty = new ApiPerformance.GoalRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                    GoalId = (request.Goal?.GoalId ?? ""),
                },
            },
        };
        using var rpc1 = _performanceReviewWorkflowService.StartReviewWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.PauseGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
                Priority = (request.Goal?.Priority ?? 0),
            },
            Outcome = new ApiPerformance.PauseGoalWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
                Status = (reply1.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "PauseGoalWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Retrys a goal.
    /// </summary>
    public override async Task<ApiPerformance.RetryGoalWorkflowResponse> RetryGoalWorkflow(ApiPerformance.RetryGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling RetryGoalWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ApproveGoalRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    Email = (request.Details?.Notify?.Email ?? ""),
                },
            },
        };
        var json0 = JsonFormatter.Default.Format(input0);
        input0 = JsonParser.Default.Parse<ApiPerformance.ApproveGoalRequest>(json0);
        using var rpc0 = _performanceGoalApprovalService.ApproveGoalAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.AdvanceRegistrationWorkflowRequest();
        input1.Context = (request.Context ?? new ApiCommon.RequestContext());
        input1.Registration = new ApiCorporateevents.Registration
        {
            Audit = new ApiCommon.AuditInfo
            {
                CreatedAt = (reply0.Goal?.Audit?.CreatedAt ?? new Timestamp()),
            },
        };
        using var rpc1 = _corporateeventsRegistrationWorkflowService.AdvanceRegistrationWorkflowAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = new ApiRecruiting.WatchJobOpeningsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Subscriber = new ApiCommon.ContactInfo
            {
                Email = (reply0.Goal?.Assignee?.Email ?? ""),
                Phone = (request.Goal?.Requester?.Phone ?? ""),
                PreferredChannel = (request.Goal?.Requester?.PreferredChannel ?? ""),
            },
        };
        using var stream2 = _recruitingJobOpeningService.WatchJobOpenings(input2, cancellationToken: token);
        await foreach (var change2 in stream2.ResponseStream.ReadAllAsync(token))
        {
            _logger.LogDebug("Received WatchJobOpenings frame {Type}", change2.GetType().Name);
        }
        var input3 = new ApiRecruiting.QuoteOfferRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Offer = new ApiRecruiting.Offer
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Goal?.Requester?.Email ?? ""),
                },
            },
        };
        var reply3 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _recruitingOfferCalculationService.QuoteOfferAsync(input3, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var response = new ApiPerformance.RetryGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                CustomFields = { (request.Goal?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                GoalId = (request.GoalId ?? ""),
                Requester = new ApiCommon.ContactInfo
                {
                    Email = (reply1.Registration?.Contact?.Email ?? ""),
                },
                Sla = new ApiPerformance.GoalSla
                {
                    Breached = (reply0.Goal?.Sla?.Breached ?? false),
                    DueAt = (reply3.Offer?.DueAt ?? new Timestamp()),
                },
            },
            Outcome = new ApiPerformance.RetryGoalWorkflowResponse.Types.Outcome
            {
                Message = (reply3.Result?.Message ?? ""),
                ReferenceId = (reply1.Outcome?.ReferenceId ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "RetryGoalWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Signals a goal.
    /// </summary>
    public override async Task<ApiPerformance.SignalGoalWorkflowResponse> SignalGoalWorkflow(ApiPerformance.SignalGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SignalGoalWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiDocuments.QuoteSignatureRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Signature = new ApiDocuments.Signature
            {
                Detail = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Subject = new ApiDocuments.DocumentRef
                {
                    DisplayName = (request.Goal ?? new ApiPerformance.Goal()).OriginCase switch
                    {
                        ApiPerformance.Goal.OriginOneofCase.Customer => (request.Goal?.Customer?.DisplayName ?? ""),
                        _ => ""
                    },
                },
            },
        };
        using var rpc0 = _documentsSignatureCalculationService.QuoteSignatureAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiPerformance.SignalGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                GoalId = (request.GoalId ?? ""),
                Requester = (request.Goal?.Requester ?? new ApiCommon.ContactInfo()),
            },
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply0.Signature?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "SignalGoalWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    /// <summary>
    /// Starts a goal.
    /// </summary>
    public override async Task<ApiPerformance.StartGoalWorkflowResponse> StartGoalWorkflow(ApiPerformance.StartGoalWorkflowRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling StartGoalWorkflow for {Tenant}", requestContext.TenantId);
        if (request.Goal is { } record)
        {
            var snapshot = GoalMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiPerformance.ListReviewWorkflowsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Filter = new ApiPerformance.ListReviewWorkflowsRequest.Types.Filter
            {
                CreatedAfter = (request.Goal?.Audit?.CreatedAt ?? new Timestamp()),
                OwnerId = (request.Goal?.Sla?.PolicyId ?? ""),
            },
        };
        var reply0 = await Workflow.SendAsync(input0, token, _performanceReviewWorkflowService.ListReviewWorkflowsAsync);
        var input1 = new ApiDocuments.RetryDocumentFolderExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            UpdatedAt = (request.Goal?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.Goal?.Assignee?.DisplayName ?? ""),
                            Email = (request.Details?.Notify?.Email ?? ""),
                            UserId = (request.Goal?.Assignee?.UserId ?? ""),
                        },
                    } },
                } },
            },
        };
        using var rpc1 = _sendStartGoalWorkflow1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiPerformance.StartGoalWorkflowResponse
        {
            Goal = new ApiPerformance.Goal
            {
                Assignee = new ApiIdentity.UserRef
                {
                    UserId = (reply1.DocumentFolder?.Manager?.UserId ?? ""),
                },
                CustomFields = { (request.Goal?.CustomFields ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                GoalId = (request.GoalId ?? ""),
            },
            Outcome = new ApiPerformance.StartGoalWorkflowResponse.Types.Outcome
            {
                Status = (reply0.Result?.Status ?? ""),
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply0.Result?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Reason ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "goal",
            "audit", "StartGoalWorkflow", (request.Reason ?? ""), token);
        return response;
    }
    private async Task<ApiPerformance.ResolveGoalsResponse> SendListGoalWorkflows0Async(ApiPerformance.ResolveGoalsRequest request, CancellationToken token)
    {
        return await _goalWorkflowEndpointSendListGoalWorkflows0Port.SendAsync(request, token);
    }
}
