using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiCommon = Acme.Common.V1;
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Programs.DirectorySweep;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        builder.Services.AddWorkforcePlatform(builder.Configuration);
        using var host = builder.Build();
        using var deadline = new CancellationTokenSource(TimeSpan.FromHours(2));
        var token = deadline.Token;
        var tenant = builder.Configuration["Tenant"] ?? throw new InvalidOperationException("Set --Tenant");
        // The program calls with the platform's workload credential, so it acts as its service account.
        var context = new Acme.Common.V1.RequestContext
        {
            TenantId = tenant,
            Actor = new Acme.Common.V1.Actor { UserId = "workforce-directory-sweep", ServiceAccount = true },
        };
        await RunCorporateevents1Async(host.Services, context, token);
        await RunEmployees5Async(host.Services, context, token);
        await RunWorkplaces16Async(host.Services, context, token);
    }
    private static async Task RunCorporateevents1Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiCorporateevents.CorporateEventApprovalService.CorporateEventApprovalServiceClient>();
        var writer = provider.GetRequiredService<ApiCorporateevents.CorporateEventService.CorporateEventServiceClient>();
        var request = new ApiCorporateevents.ListPendingCorporateEventsRequest
        {
            Context = context,
        };
        request.Page = new ApiCommon.PageRequest { PageSize = 200 };
        do
        {
            using var query = reader.ListPendingCorporateEventsAsync(request, cancellationToken: token);
            var page = await query.ResponseAsync;
            foreach (var item in page.CorporateEvents)
            {
                using var update = writer.UpdateCorporateEventAsync(new ApiCorporateevents.UpdateCorporateEventRequest
                {
                    Context = context,
                    CorporateEvent = item.Clone(),
                }, cancellationToken: token);
                await update.ResponseAsync;
            }
            request.Page.PageToken = page.PageInfo?.NextPageToken ?? "";
        } while (request.Page.PageToken.Length != 0);
    }
    private static async Task RunEmployees5Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiEmployees.EmployeeApprovalService.EmployeeApprovalServiceClient>();
        var writer = provider.GetRequiredService<ApiEmployees.EmployeeService.EmployeeServiceClient>();
        var request = new ApiEmployees.ListPendingEmployeesRequest
        {
            Context = context,
        };
        request.Page = new ApiCommon.PageRequest { PageSize = 200 };
        do
        {
            using var query = reader.ListPendingEmployeesAsync(request, cancellationToken: token);
            var page = await query.ResponseAsync;
            foreach (var item in page.Employees)
            {
                using var update = writer.UpdateEmployeeAsync(new ApiEmployees.UpdateEmployeeRequest
                {
                    Context = context,
                    Employee = item.Clone(),
                }, cancellationToken: token);
                await update.ResponseAsync;
            }
            request.Page.PageToken = page.PageInfo?.NextPageToken ?? "";
        } while (request.Page.PageToken.Length != 0);
    }
    private static async Task RunWorkplaces16Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiWorkplaces.BuildingApprovalService.BuildingApprovalServiceClient>();
        var writer = provider.GetRequiredService<ApiWorkplaces.BuildingService.BuildingServiceClient>();
        var request = new ApiWorkplaces.ListPendingBuildingsRequest
        {
            Context = context,
        };
        request.Page = new ApiCommon.PageRequest { PageSize = 200 };
        do
        {
            using var query = reader.ListPendingBuildingsAsync(request, cancellationToken: token);
            var page = await query.ResponseAsync;
            foreach (var item in page.Buildings)
            {
                using var update = writer.UpdateBuildingAsync(new ApiWorkplaces.UpdateBuildingRequest
                {
                    Building = item.Clone(),
                    Context = context,
                }, cancellationToken: token);
                await update.ResponseAsync;
            }
            request.Page.PageToken = page.PageInfo?.NextPageToken ?? "";
        } while (request.Page.PageToken.Length != 0);
    }
}
