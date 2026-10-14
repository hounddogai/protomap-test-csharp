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

namespace Acme.Workforce.Licensing;

/// <summary>
/// Registers the licensing domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddLicensing(this IServiceCollection services)
    {
        services.AddTransient<EntitlementApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, EntitlementApprovalEndpointSubmitEntitlement0NotificationHandler>();
        services.AddTransient<EntitlementCalculationEndpoint>();
        services.AddTransient<IEntitlementCalculationEndpointSendExplainEntitlementCalculation0Port, EntitlementCalculationEndpointSendExplainEntitlementCalculation0PortAdapter>();
        services.AddTransient<EntitlementLifecycleEndpoint>();
        services.AddTransient<EntitlementQueryEndpoint>();
        services.AddTransient<EntitlementValidationEndpoint>();
        services.AddTransient<IEntitlementValidationEndpointSendExplainEntitlementDecision3Port, EntitlementValidationEndpointSendExplainEntitlementDecision3PortAdapter>();
        services.AddTransient<EntitlementWorkflowEndpoint>();
        services.AddTransient<LicenseAssignmentApprovalEndpoint>();
        services.AddTransient<ILicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3Port, LicenseAssignmentApprovalEndpointSendEscalateLicenseAssignment3PortAdapter>();
        services.AddSingleton<INotificationHandler, LicenseAssignmentApprovalEndpointSubmitLicenseAssignment3NotificationHandler>();
        services.AddTransient<LicenseAssignmentCalculationEndpoint>();
        services.AddTransient<LicenseAssignmentLifecycleEndpoint>();
        services.AddTransient<ILicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0Port, LicenseAssignmentLifecycleEndpointSendReinstateLicenseAssignment0PortAdapter>();
        services.AddTransient<LicenseAssignmentReportingEndpoint>();
        services.AddTransient<ILicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0Port, LicenseAssignmentReportingEndpointSendFetchLicenseAssignmentReport0PortAdapter>();
        services.AddSingleton<INotificationHandler, LicenseAssignmentReportingEndpointShareLicenseAssignmentReport1NotificationHandler>();
        services.AddTransient<LicenseAssignmentValidationEndpoint>();
        services.AddTransient<ILicenseAssignmentValidationEndpointSendLintLicenseAssignments1Port, LicenseAssignmentValidationEndpointSendLintLicenseAssignments1PortAdapter>();
        services.AddTransient<LicenseAssignmentWorkflowEndpoint>();
        services.AddTransient<ILicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3Port, LicenseAssignmentWorkflowEndpointSendCancelLicenseAssignmentWorkflow3PortAdapter>();
        services.AddTransient<SoftwareLicenseApprovalEndpoint>();
        services.AddTransient<ISoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2Port, SoftwareLicenseApprovalEndpointSendRecallSoftwareLicense2PortAdapter>();
        services.AddSingleton<INotificationHandler, SoftwareLicenseApprovalEndpointSubmitSoftwareLicense0NotificationHandler>();
        services.AddTransient<SoftwareLicenseCalculationEndpoint>();
        services.AddTransient<SoftwareLicenseQueryEndpoint>();
        services.AddTransient<SoftwareLicenseEndpoint>();
        services.AddSingleton<INotificationHandler, SoftwareLicenseEndpointWatchSoftwareLicenses0NotificationHandler>();
        services.AddTransient<SoftwareLicenseValidationEndpoint>();
        services.AddTransient<ISoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0Port, SoftwareLicenseValidationEndpointSendCheckSoftwareLicenseEligibility0PortAdapter>();
        services.AddTransient<ISoftwareLicenseValidationEndpointSendValidateSoftwareLicense2Port, SoftwareLicenseValidationEndpointSendValidateSoftwareLicense2PortAdapter>();
        services.AddTransient<SoftwareLicenseWorkflowEndpoint>();
        return services;
    }
    public static void MapLicensing(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<EntitlementApprovalEndpoint>();
        routes.MapGrpcService<EntitlementCalculationEndpoint>();
        routes.MapGrpcService<EntitlementLifecycleEndpoint>();
        routes.MapGrpcService<EntitlementQueryEndpoint>();
        routes.MapGrpcService<EntitlementValidationEndpoint>();
        routes.MapGrpcService<EntitlementWorkflowEndpoint>();
        routes.MapGrpcService<LicenseAssignmentApprovalEndpoint>();
        routes.MapGrpcService<LicenseAssignmentCalculationEndpoint>();
        routes.MapGrpcService<LicenseAssignmentLifecycleEndpoint>();
        routes.MapGrpcService<LicenseAssignmentReportingEndpoint>();
        routes.MapGrpcService<LicenseAssignmentValidationEndpoint>();
        routes.MapGrpcService<LicenseAssignmentWorkflowEndpoint>();
        routes.MapGrpcService<SoftwareLicenseApprovalEndpoint>();
        routes.MapGrpcService<SoftwareLicenseCalculationEndpoint>();
        routes.MapGrpcService<SoftwareLicenseQueryEndpoint>();
        routes.MapGrpcService<SoftwareLicenseEndpoint>();
        routes.MapGrpcService<SoftwareLicenseValidationEndpoint>();
        routes.MapGrpcService<SoftwareLicenseWorkflowEndpoint>();
    }
}
