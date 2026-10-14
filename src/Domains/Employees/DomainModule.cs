using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.Workforce.Employees;

/// <summary>
/// Registers the employees domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddEmployees(this IServiceCollection services)
    {
        services.AddTransient<EmployeeApprovalEndpoint>();
        services.AddTransient<EmployeeQueryEndpoint>();
        services.AddTransient<EmployeeEndpoint>();
        services.AddTransient<EmployeeWorkflowEndpoint>();
        services.AddTransient<IEmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0Port, EmployeeWorkflowEndpointSendAdvanceEmployeeWorkflow0PortAdapter>();
        services.AddSingleton<INotificationHandler, EmployeeWorkflowEndpointCancelEmployeeWorkflow1NotificationHandler>();
        services.AddTransient<EmploymentContractApprovalEndpoint>();
        services.AddTransient<IEmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2Port, EmploymentContractApprovalEndpointSendDelegateEmploymentContractApproval2PortAdapter>();
        services.AddSingleton<INotificationHandler, EmploymentContractApprovalEndpointSubmitEmploymentContract2NotificationHandler>();
        services.AddTransient<EmploymentContractCalculationEndpoint>();
        services.AddTransient<EmploymentContractValidationEndpoint>();
        services.AddTransient<IEmploymentContractValidationEndpointSendNormalizeEmploymentContract3Port, EmploymentContractValidationEndpointSendNormalizeEmploymentContract3PortAdapter>();
        services.AddTransient<OrgUnitApprovalEndpoint>();
        services.AddTransient<IOrgUnitApprovalEndpointSendEscalateOrgUnit2Port, OrgUnitApprovalEndpointSendEscalateOrgUnit2PortAdapter>();
        services.AddSingleton<INotificationHandler, OrgUnitApprovalEndpointCommentOrgUnitApproval0NotificationHandler>();
        services.AddTransient<OrgUnitQueryEndpoint>();
        services.AddTransient<IOrgUnitQueryEndpointSendFindOrgUnits0Port, OrgUnitQueryEndpointSendFindOrgUnits0PortAdapter>();
        services.AddTransient<OrgUnitWorkflowEndpoint>();
        services.AddTransient<IOrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1Port, OrgUnitWorkflowEndpointSendRetryOrgUnitWorkflow1PortAdapter>();
        services.AddTransient<PositionApprovalEndpoint>();
        services.AddTransient<IPositionApprovalEndpointSendRejectPosition0Port, PositionApprovalEndpointSendRejectPosition0PortAdapter>();
        services.AddTransient<PositionValidationEndpoint>();
        services.AddTransient<PositionWorkflowEndpoint>();
        return services;
    }
    public static void MapEmployees(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<EmployeeApprovalEndpoint>();
        routes.MapGrpcService<EmployeeQueryEndpoint>();
        routes.MapGrpcService<EmployeeEndpoint>();
        routes.MapGrpcService<EmployeeWorkflowEndpoint>();
        routes.MapGrpcService<EmploymentContractApprovalEndpoint>();
        routes.MapGrpcService<EmploymentContractCalculationEndpoint>();
        routes.MapGrpcService<EmploymentContractValidationEndpoint>();
        routes.MapGrpcService<OrgUnitApprovalEndpoint>();
        routes.MapGrpcService<OrgUnitQueryEndpoint>();
        routes.MapGrpcService<OrgUnitWorkflowEndpoint>();
        routes.MapGrpcService<PositionApprovalEndpoint>();
        routes.MapGrpcService<PositionValidationEndpoint>();
        routes.MapGrpcService<PositionWorkflowEndpoint>();
    }
}
