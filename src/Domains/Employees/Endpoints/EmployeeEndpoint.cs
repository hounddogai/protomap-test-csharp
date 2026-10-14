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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiExperiments = Acme.Experiments.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiVenues = Acme.Venues.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

/// <summary>
/// Manages employees: creates, reads, changes, and lists them.
/// </summary>
public sealed class EmployeeEndpoint : ApiEmployees.EmployeeService.EmployeeServiceBase
{
    private readonly ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient _corporateeventsCorporateEventCalculationService;
    private readonly ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient _corporateeventsRegistrationCalculationService;
    private readonly ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient _documentsSignatureNotificationService;
    private readonly ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient _employeesEmploymentContractCalculationService;
    private readonly ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient _employeesOrgUnitQueryService;
    private readonly ApiEmployees.PositionValidationService.PositionValidationServiceClient _employeesPositionValidationService;
    private readonly ApiExperiments.ExperimentService.ExperimentServiceClient _experimentsExperimentService;
    private readonly CallInvoker _invoker;
    private readonly ILogger<EmployeeEndpoint> _logger;
    private readonly ClientRegistry _registry;
    private readonly IRecordRepository _repository;
    private readonly ApiScheduling.RosterValidationService.RosterValidationServiceClient _schedulingRosterValidationService;
    private readonly Func<ApiCorporateevents.CalculateRegistrationRequest, CallOptions, AsyncUnaryCall<ApiCorporateevents.CalculateRegistrationResponse>> _sendDeleteEmployee1;
    private readonly IOptions<MeshOptions> _settings;
    private readonly ApiVenues.VenueQueryService.VenueQueryServiceClient _venuesVenueQueryService;
    private readonly ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient _workplacesDeskCalculationService;

    public EmployeeEndpoint(
        ApiCorporateevents.CorporateEventCalculationService.CorporateEventCalculationServiceClient corporateeventsCorporateEventCalculationService,
        ApiCorporateevents.RegistrationCalculationService.RegistrationCalculationServiceClient corporateeventsRegistrationCalculationService,
        ApiDocuments.SignatureNotificationService.SignatureNotificationServiceClient documentsSignatureNotificationService,
        ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient employeesEmploymentContractCalculationService,
        ApiEmployees.OrgUnitQueryService.OrgUnitQueryServiceClient employeesOrgUnitQueryService,
        ApiEmployees.PositionValidationService.PositionValidationServiceClient employeesPositionValidationService,
        ApiExperiments.ExperimentService.ExperimentServiceClient experimentsExperimentService,
        CallInvoker invoker,
        ILogger<EmployeeEndpoint> logger,
        ClientRegistry registry,
        IRecordRepository repository,
        ApiScheduling.RosterValidationService.RosterValidationServiceClient schedulingRosterValidationService,
        IOptions<MeshOptions> settings,
        ApiVenues.VenueQueryService.VenueQueryServiceClient venuesVenueQueryService,
        ApiWorkplaces.DeskCalculationService.DeskCalculationServiceClient workplacesDeskCalculationService)
    {
        _corporateeventsCorporateEventCalculationService = corporateeventsCorporateEventCalculationService;
        _corporateeventsRegistrationCalculationService = corporateeventsRegistrationCalculationService;
        _documentsSignatureNotificationService = documentsSignatureNotificationService;
        _employeesEmploymentContractCalculationService = employeesEmploymentContractCalculationService;
        _employeesOrgUnitQueryService = employeesOrgUnitQueryService;
        _employeesPositionValidationService = employeesPositionValidationService;
        _experimentsExperimentService = experimentsExperimentService;
        _invoker = invoker;
        _logger = logger;
        _registry = registry;
        _repository = repository;
        _schedulingRosterValidationService = schedulingRosterValidationService;
        _settings = settings;
        _venuesVenueQueryService = venuesVenueQueryService;
        _workplacesDeskCalculationService = workplacesDeskCalculationService;
        _sendDeleteEmployee1 = _corporateeventsRegistrationCalculationService.CalculateRegistrationAsync;
    }
    /// <summary>
    /// Takes a stream of employees and answers once at the end.
    /// </summary>
    public override async Task<ApiEmployees.BatchCreateEmployeesResponse> BatchCreateEmployees(IAsyncStreamReader<ApiEmployees.BatchCreateEmployeesRequest> requests, ServerCallContext context)
    {
        var token = context.CancellationToken;
        var result = new ApiEmployees.BatchCreateEmployeesResponse();
        await foreach (var request in requests.ReadAllAsync(token))
        {
            RequestGuard.Tenant(request.Context);
            var requestContext = request.Context;
            using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
                requestContext.TenantId, requestContext.TraceId);
            _logger.LogInformation("Handling BatchCreateEmployees for {Tenant}", requestContext.TenantId);
            if (request.Record is { } record)
            {
                var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
                await _repository.SaveAsync(snapshot, token);
            }
            var input0 = new ApiScheduling.PreviewRosterRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiScheduling.PreviewRosterRequest.Types.Details
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Name = request.ChunkCase switch
                        {
                            ApiEmployees.BatchCreateEmployeesRequest.ChunkOneofCase.Header => (request.Header?.SubmittedBy?.Name ?? new ApiCommon.PersonName()),
                            _ => new ApiCommon.PersonName()
                        },
                        PreferredChannel = request.ChunkCase switch
                        {
                            ApiEmployees.BatchCreateEmployeesRequest.ChunkOneofCase.Record => (request.Record?.Contact?.PreferredChannel ?? ""),
                            _ => ""
                        },
                    },
                },
                Roster = new ApiScheduling.Roster
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        UpdatedBy = request.ChunkCase switch
                        {
                            ApiEmployees.BatchCreateEmployeesRequest.ChunkOneofCase.Record => (request.Record?.Audit?.UpdatedBy ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc0 = _schedulingRosterValidationService.PreviewRosterAsync(input0, cancellationToken: token);
            var reply0 = await rpc0.ResponseAsync;
            var input1 = new ApiEmployees.CalculateEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiEmployees.CalculateEmploymentContractRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Recipient = (reply0.Roster?.BillingAddress?.Recipient ?? ""),
                    },
                },
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    BillingAddress = new ApiCommon.PostalAddress
                    {
                        Lines = { (reply0.Roster?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                    },
                },
            };
            var labels1 = input1.Context.Baggage.Select(pair => new { pair.Key, pair.Value })
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            input1.Context.Baggage.Clear();
            input1.Context.Baggage.Add(labels1);
            using var rpc1 = _employeesEmploymentContractCalculationService.CalculateEmploymentContractAsync(input1, cancellationToken: token);
            var reply1 = await rpc1.ResponseAsync;
            var input2 = new ApiEmployees.QuoteEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                Details = new ApiEmployees.QuoteEmploymentContractRequest.Types.Details
                {
                    Destination = new ApiCommon.PostalAddress
                    {
                        Locality = request.ChunkCase switch
                        {
                            ApiEmployees.BatchCreateEmployeesRequest.ChunkOneofCase.Record => (request.Record?.PrimaryAddress?.Locality ?? ""),
                            _ => ""
                        },
                        Region = (reply1.EmploymentContract?.BillingAddress?.Region ?? ""),
                    },
                },
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Audit = new ApiCommon.AuditInfo
                    {
                        CreatedAt = (reply1.EmploymentContract?.Audit?.CreatedAt ?? new Timestamp()),
                        UpdatedBy = (reply1.EmploymentContract?.Audit?.UpdatedBy ?? ""),
                    },
                },
            };
            using var rpc2 = _employeesEmploymentContractCalculationService.QuoteEmploymentContractAsync(input2, cancellationToken: token);
            var reply2 = await rpc2.ResponseAsync;
            var input3 = new ApiEmployees.EstimateEmploymentContractRequest
            {
                Context = (request.Context ?? new ApiCommon.RequestContext()),
                EmploymentContract = new ApiEmployees.EmploymentContract
                {
                    Bank = new ApiEmployees.EmploymentContractBankSettlement
                    {
                        AccountHolder = (reply2.EmploymentContract ?? new ApiEmployees.EmploymentContract()).PaymentCase switch
                        {
                            ApiEmployees.EmploymentContract.SettlementOneofCase.Bank => (reply2.EmploymentContract?.Bank?.AccountHolder ?? ""),
                            _ => ""
                        },
                    },
                },
            };
            using var rpc3 = _employeesEmploymentContractCalculationService.EstimateEmploymentContractAsync(input3, cancellationToken: token);
            var reply3 = await rpc3.ResponseAsync;
            var response = new ApiEmployees.BatchCreateEmployeesResponse
            {
                Result = new ApiCommon.OperationResult
                {
                    CorrelationId = (reply3.Result?.CorrelationId ?? ""),
                    Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                    Message = (reply3.Result?.Message ?? ""),
                    ReferenceIds = { (reply1.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            };
            _logger.LogDebug("Local audit detail {Detail}", (request.Record?.TaxNumber ?? ""));
            await _repository.RecordAsync(requestContext.TenantId, "employee",
                "audit", "BatchCreateEmployees", (request.Record?.TaxNumber ?? ""), token);
            result = response;
        }
        return result;
    }
    /// <summary>
    /// Returns several employees at once.
    /// </summary>
    public override async Task<ApiEmployees.BatchGetEmployeesResponse> BatchGetEmployees(ApiEmployees.BatchGetEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling BatchGetEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "BatchGetEmployees", "accepted", token);
        var input0 = new ApiVenues.CountVenuesRequest();
        input0.Context = (request.Context ?? new ApiCommon.RequestContext());
        input0.Filter = new ApiVenues.CountVenuesRequest.Types.Filter
        {
            CreatedAfter = (request.Context?.Deadline ?? new Timestamp()),
        };
        using var rpc0 = _venuesVenueQueryService.CountVenuesAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.BatchGetEmployeesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Creates an employee.
    /// </summary>
    public override async Task<ApiEmployees.CloneEmployeeResponse> CloneEmployee(ApiEmployees.CloneEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CloneEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.ExplainEmploymentContractCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.ExplainEmploymentContractCalculationRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Employee?.RelationshipManager?.Email ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (request.Employee?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Recipient = (request.Employee?.PrimaryAddress?.Recipient ?? ""),
                },
            },
        };
        using var rpc0 = _employeesEmploymentContractCalculationService.ExplainEmploymentContractCalculationAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.CloneEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = (request.Employee?.Contact ?? new ApiCommon.ContactInfo()),
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    PostalCode = (reply0.EmploymentContract?.BillingAddress?.PostalCode ?? ""),
                },
                RelationshipManager = (request.Employee?.RelationshipManager ?? new ApiIdentity.UserRef()),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "CloneEmployee", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Creates an employee.
    /// </summary>
    public override async Task<ApiEmployees.CreateEmployeeResponse> CreateEmployee(ApiEmployees.CreateEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling CreateEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiWorkplaces.EstimateDeskRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiWorkplaces.EstimateDeskRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Employee?.PrimaryAddress?.CountryCode ?? ""),
                    Lines = { (request.Employee?.PrimaryAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                },
            },
        };
        var reply0 = await Workflow.RunAsync(async cancellation =>
        {
            using var rpc = _workplacesDeskCalculationService.EstimateDeskAsync(input0, cancellationToken: cancellation);
            return await rpc.ResponseAsync.ConfigureAwait(false);
        }, token);
        var input1 = new ApiDocuments.UnsubscribeSignatureUpdatesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiDocuments.UnsubscribeSignatureUpdatesRequest.Types.Details
            {
                Payload = Any.Pack((request.Context ?? new ApiCommon.RequestContext())),
                Recipient = new ApiCommon.ContactInfo
                {
                    Email = (request.Employee?.Contact?.Email ?? ""),
                    Name = (request.Employee?.Contact?.Name ?? new ApiCommon.PersonName()),
                    Phone = (request.Employee?.Contact?.Phone ?? ""),
                    PreferredChannel = (request.Employee?.Contact?.PreferredChannel ?? ""),
                },
            },
        };
        using var rpc1 = _documentsSignatureNotificationService.UnsubscribeSignatureUpdatesAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.CreateEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Desk?.Owner?.Email ?? ""),
                },
                EmployeeId = (request.Employee?.EmployeeId ?? ""),
                PrimaryAddress = (request.Employee?.PrimaryAddress ?? new ApiCommon.PostalAddress()),
                RelationshipManager = new ApiIdentity.UserRef
                {
                    Email = (reply0.Desk?.Owner?.Email ?? ""),
                },
            },
            Result = new ApiCommon.OperationResult
            {
                Message = (reply1.Outcome?.Message ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.RequestId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "CreateEmployee", (request.RequestId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Deletes an employee.
    /// </summary>
    public override async Task<ApiEmployees.DeleteEmployeeResponse> DeleteEmployee(ApiEmployees.DeleteEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmployeeId, "employee_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling DeleteEmployee for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", request.EmployeeId,
            "DeleteEmployee", "accepted", token);
        var input0 = new ApiEmployees.PreviewPositionRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.PreviewPositionRequest.Types.Details
            {
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
            Position = new ApiEmployees.Position
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (request.Etag ?? ""),
                },
                Owner = new ApiIdentity.UserRef
                {
                    UserId = (request.Context?.Actor?.UserId ?? ""),
                },
            },
            Reason = (request.Reason ?? ""),
        };
        var reply0 = await Workflow.SendAsync(input0, token, _employeesPositionValidationService.PreviewPositionAsync);
        var input1 = new ApiCorporateevents.CalculateRegistrationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Registration = new ApiCorporateevents.Registration
            {
                Audit = new ApiCommon.AuditInfo
                {
                    Etag = (reply0.Position?.Audit?.Etag ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (reply0.Position?.Owner?.Email ?? ""),
                },
            },
        };
        using var rpc1 = _sendDeleteEmployee1(input1, new CallOptions(cancellationToken: token));
        var reply1 = await rpc1.ResponseAsync;
        var response = new ApiEmployees.DeleteEmployeeResponse
        {
            EffectiveAt = (reply0.Outcome?.EffectiveAt ?? new Timestamp()),
            Result = new ApiCommon.OperationResult
            {
                CorrelationId = (reply1.Result?.CorrelationId ?? ""),
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.EmployeeId ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "DeleteEmployee", (request.EmployeeId ?? ""), token);
        return response;
    }
    /// <summary>
    /// Returns an employee.
    /// </summary>
    public override async Task<ApiEmployees.GetEmployeeResponse> GetEmployee(ApiEmployees.GetEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        RequestGuard.Identifier(request.EmployeeId, "employee_id");
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling GetEmployee for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", request.EmployeeId,
            "GetEmployee", "accepted", token);
        var response = new ApiEmployees.GetEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                EmployeeId = (request.EmployeeId ?? ""),
            },
        };
        return response;
    }
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.ListEmployeesResponse> ListEmployees(ApiEmployees.ListEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling ListEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "ListEmployees", "accepted", token);
        var input0 = new ApiEmployees.CompareEmploymentContractOptionsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedAt = (request.Filter?.CreatedAfter ?? new Timestamp()),
                },
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
        };
        var named0 = _registry.Get<ApiEmployees.EmploymentContractCalculationService.EmploymentContractCalculationServiceClient>("acme.employees.v1.EmploymentContractCalculationService");
        using var rpc0 = named0.CompareEmploymentContractOptionsAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.ListEmployeesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "ListEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Lists employees a page at a time.
    /// </summary>
    public override async Task<ApiEmployees.SearchEmployeesResponse> SearchEmployees(ApiEmployees.SearchEmployeesRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling SearchEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "SearchEmployees", "accepted", token);
        var input0 = new ApiEmployees.RecalculateEmploymentContractsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Details = new ApiEmployees.RecalculateEmploymentContractsRequest.Types.Details
            {
                Destination = new ApiCommon.PostalAddress
                {
                    Region = (request.Filter?.Region ?? ""),
                },
            },
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                Counterparty = new ApiEmployees.EmployeeRef
                {
                    Email = (request.Context?.Actor?.Email ?? ""),
                },
            },
        };
        var descriptor0 = new Method<ApiEmployees.RecalculateEmploymentContractsRequest, ApiEmployees.RecalculateEmploymentContractsResponse>(
            MethodType.Unary, "acme.employees.v1" + ".EmploymentContractCalculationService", "RecalculateEmploymentContracts",
            Wire.Marshaller<ApiEmployees.RecalculateEmploymentContractsRequest>(), Wire.Marshaller<ApiEmployees.RecalculateEmploymentContractsResponse>());
        using var rpc0 = _invoker.AsyncUnaryCall(descriptor0, null,
            new CallOptions(cancellationToken: token), input0);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.SearchEmployeesResponse
        {
            Result = new ApiCommon.OperationResult
            {
                Details = { (reply0.Result?.Details ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                ReferenceIds = { (reply0.Result?.ReferenceIds ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Page?.PageToken ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "SearchEmployees", (request.Page?.PageToken ?? ""), token);
        return response;
    }
    /// <summary>
    /// Changes an employee.
    /// </summary>
    public override async Task<ApiEmployees.UpdateEmployeeResponse> UpdateEmployee(ApiEmployees.UpdateEmployeeRequest request, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling UpdateEmployee for {Tenant}", requestContext.TenantId);
        if (request.Employee is { } record)
        {
            var snapshot = EmployeeMapper.ToRecord(requestContext.TenantId, record);
            await _repository.SaveAsync(snapshot, token);
        }
        var input0 = new ApiEmployees.SimulateEmploymentContractRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            EmploymentContract = new ApiEmployees.EmploymentContract
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (request.Employee?.PrimaryAddress?.CountryCode ?? ""),
                },
            },
        };
        var command0 = new DispatchCommand<ApiEmployees.SimulateEmploymentContractRequest>(input0,
            "primary", requestContext.TenantId);
        var routed0 = command0 with { Region = _settings.Value.Region };
        using var rpc0 = _employeesEmploymentContractCalculationService.SimulateEmploymentContractAsync(routed0.Payload, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var response = new ApiEmployees.UpdateEmployeeResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Attributes = { (request.Employee?.Attributes ?? new Google.Protobuf.Collections.MapField<string, string>()) },
                Audit = new ApiCommon.AuditInfo
                {
                    CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Name = (request.Employee?.Name ?? new ApiCommon.PersonName()),
                },
                PrimaryAddress = new ApiCommon.PostalAddress
                {
                    CountryCode = (reply0.EmploymentContract?.BillingAddress?.CountryCode ?? ""),
                },
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Employee?.TaxNumber ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "UpdateEmployee", (request.Employee?.TaxNumber ?? ""), token);
        return response;
    }
    /// <summary>
    /// Streams the changes of employees as they happen.
    /// </summary>
    public override async Task WatchEmployees(ApiEmployees.WatchEmployeesRequest request, IServerStreamWriter<ApiEmployees.WatchEmployeesResponse> responses, ServerCallContext context)
    {
        var token = context.CancellationToken;
        RequestGuard.Tenant(request.Context);
        var requestContext = request.Context;
        using var scope = _logger.BeginScope("Tenant {Tenant} Trace {Trace}",
            requestContext.TenantId, requestContext.TraceId);
        _logger.LogInformation("Handling WatchEmployees for {Tenant}", requestContext.TenantId);
        await _repository.RecordAsync(requestContext.TenantId, "employee", "batch",
            "WatchEmployees", "accepted", token);
        var profile0 = new MappingProfile<ApiEmployees.WatchEmployeesRequest, ApiExperiments.CloneExperimentRequest>(value => new ApiExperiments.CloneExperimentRequest
        {
            Context = (value.Context ?? new ApiCommon.RequestContext()),
            Experiment = new ApiExperiments.Experiment
            {
                Organizer = new ApiIdentity.UserRef
                {
                    Email = (value.Subscriber?.Email ?? ""),
                },
                Participants = { new ApiExperiments.ExperimentParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        PreferredChannel = (value.Subscriber?.PreferredChannel ?? ""),
                    },
                } },
            },
        });
        var input0 = profile0.Map(request);
        using var rpc0 = _experimentsExperimentService.CloneExperimentAsync(input0, cancellationToken: token);
        var reply0 = await rpc0.ResponseAsync;
        var input1 = new ApiCorporateevents.ExplainCorporateEventCalculationRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            CorporateEvent = new ApiCorporateevents.CorporateEvent
            {
                Participants = { new ApiCorporateevents.CorporateEventParticipant
                {
                    Contact = new ApiCommon.ContactInfo
                    {
                        Email = (reply0.Experiment?.Organizer?.Email ?? ""),
                    },
                } },
                Weekly = new ApiCorporateevents.CorporateEventWeeklyRule
                {
                    Interval = (reply0.Experiment ?? new ApiExperiments.Experiment()).RecurrenceCase switch
                    {
                        ApiExperiments.Experiment.RecurrenceOneofCase.Weekly => (reply0.Experiment?.Weekly?.Interval ?? 0),
                        _ => 0
                    },
                },
            },
        };
        using var rpc1 = _corporateeventsCorporateEventCalculationService.ExplainCorporateEventCalculationAsync(input1, cancellationToken: token);
        var reply1 = await rpc1.ResponseAsync;
        var input2 = request.WatchEmployeesToBrowseOrgUnits2(reply0);
        using var rpc2 = _employeesOrgUnitQueryService.BrowseOrgUnitsAsync(input2, cancellationToken: token);
        var reply2 = await rpc2.ResponseAsync;
        var response = new ApiEmployees.WatchEmployeesResponse
        {
            Employee = new ApiEmployees.Employee
            {
                Audit = new ApiCommon.AuditInfo
                {
                    UpdatedAt = (reply1.CorporateEvent?.Audit?.UpdatedAt ?? new Timestamp()),
                },
                Contact = new ApiCommon.ContactInfo
                {
                    Email = (request.Subscriber?.Email ?? ""),
                },
                Name = (request.Subscriber?.Name ?? new ApiCommon.PersonName()),
            },
            Result = new ApiCommon.OperationResult
            {
                Status = (reply0.Result?.Status ?? ""),
            },
        };
        _logger.LogDebug("Local audit detail {Detail}", (request.Subscriber?.Phone ?? ""));
        await _repository.RecordAsync(requestContext.TenantId, "employee",
            "audit", "WatchEmployees", (request.Subscriber?.Phone ?? ""), token);
        await responses.WriteAsync(response, token);
    }
}
