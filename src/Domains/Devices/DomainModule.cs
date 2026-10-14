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

namespace Acme.Workforce.Devices;

/// <summary>
/// Registers the devices domain and its gRPC routes.
/// </summary>
public static class DomainModule
{
    public static IServiceCollection AddDevices(this IServiceCollection services)
    {
        services.AddTransient<ComplianceScanApprovalEndpoint>();
        services.AddSingleton<INotificationHandler, ComplianceScanApprovalEndpointApproveComplianceScan1NotificationHandler>();
        services.AddTransient<ComplianceScanCalculationEndpoint>();
        services.AddTransient<ComplianceScanLifecycleEndpoint>();
        services.AddTransient<IComplianceScanLifecycleEndpointSendTerminateComplianceScan0Port, ComplianceScanLifecycleEndpointSendTerminateComplianceScan0PortAdapter>();
        services.AddSingleton<INotificationHandler, ComplianceScanLifecycleEndpointSuspendComplianceScan3NotificationHandler>();
        services.AddTransient<ComplianceScanValidationEndpoint>();
        services.AddTransient<IComplianceScanValidationEndpointSendValidateComplianceScan1Port, ComplianceScanValidationEndpointSendValidateComplianceScan1PortAdapter>();
        services.AddTransient<ComplianceScanWorkflowEndpoint>();
        services.AddTransient<DevicePolicyApprovalEndpoint>();
        services.AddTransient<IDevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0Port, DevicePolicyApprovalEndpointSendDelegateDevicePolicyApproval0PortAdapter>();
        services.AddTransient<DevicePolicyCalculationEndpoint>();
        services.AddSingleton<INotificationHandler, DevicePolicyCalculationEndpointCompareDevicePolicyOptions0NotificationHandler>();
        services.AddTransient<DevicePolicyLifecycleEndpoint>();
        services.AddTransient<IDevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0Port, DevicePolicyLifecycleEndpointSendScheduleDevicePolicyExpiry0PortAdapter>();
        services.AddTransient<DevicePolicyQueryEndpoint>();
        services.AddTransient<IDevicePolicyQueryEndpointSendExportDevicePolicies2Port, DevicePolicyQueryEndpointSendExportDevicePolicies2PortAdapter>();
        services.AddSingleton<INotificationHandler, DevicePolicyQueryEndpointSummarizeDevicePolicies1NotificationHandler>();
        services.AddTransient<DevicePolicyValidationEndpoint>();
        services.AddTransient<IDevicePolicyValidationEndpointSendValidateDevicePolicy0Port, DevicePolicyValidationEndpointSendValidateDevicePolicy0PortAdapter>();
        services.AddTransient<DevicePolicyWorkflowEndpoint>();
        services.AddTransient<ManagedDeviceApprovalEndpoint>();
        services.AddTransient<IManagedDeviceApprovalEndpointSendRejectManagedDevice2Port, ManagedDeviceApprovalEndpointSendRejectManagedDevice2PortAdapter>();
        services.AddSingleton<INotificationHandler, ManagedDeviceApprovalEndpointRejectManagedDevice2NotificationHandler>();
        services.AddTransient<ManagedDeviceCalculationEndpoint>();
        services.AddTransient<IManagedDeviceCalculationEndpointSendSimulateManagedDevice0Port, ManagedDeviceCalculationEndpointSendSimulateManagedDevice0PortAdapter>();
        services.AddTransient<ManagedDeviceQueryEndpoint>();
        services.AddTransient<ManagedDeviceEndpoint>();
        services.AddTransient<ManagedDeviceValidationEndpoint>();
        services.AddSingleton<INotificationHandler, ManagedDeviceValidationEndpointCheckManagedDeviceEligibility1NotificationHandler>();
        services.AddTransient<ManagedDeviceWorkflowEndpoint>();
        return services;
    }
    public static void MapDevices(this IEndpointRouteBuilder routes)
    {
        routes.MapGrpcService<ComplianceScanApprovalEndpoint>();
        routes.MapGrpcService<ComplianceScanCalculationEndpoint>();
        routes.MapGrpcService<ComplianceScanLifecycleEndpoint>();
        routes.MapGrpcService<ComplianceScanValidationEndpoint>();
        routes.MapGrpcService<ComplianceScanWorkflowEndpoint>();
        routes.MapGrpcService<DevicePolicyApprovalEndpoint>();
        routes.MapGrpcService<DevicePolicyCalculationEndpoint>();
        routes.MapGrpcService<DevicePolicyLifecycleEndpoint>();
        routes.MapGrpcService<DevicePolicyQueryEndpoint>();
        routes.MapGrpcService<DevicePolicyValidationEndpoint>();
        routes.MapGrpcService<DevicePolicyWorkflowEndpoint>();
        routes.MapGrpcService<ManagedDeviceApprovalEndpoint>();
        routes.MapGrpcService<ManagedDeviceCalculationEndpoint>();
        routes.MapGrpcService<ManagedDeviceQueryEndpoint>();
        routes.MapGrpcService<ManagedDeviceEndpoint>();
        routes.MapGrpcService<ManagedDeviceValidationEndpoint>();
        routes.MapGrpcService<ManagedDeviceWorkflowEndpoint>();
    }
}
