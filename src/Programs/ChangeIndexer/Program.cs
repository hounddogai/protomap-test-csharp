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
using ApiPerformance = Acme.Performance.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Programs.ChangeIndexer;

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
            Actor = new Acme.Common.V1.Actor { UserId = "workforce-change-indexer", ServiceAccount = true },
        };
        await Task.WhenAll(
            RunCorporateevents1Async(host.Services, context, token),
            RunEmployees4Async(host.Services, context, token),
            RunPerformance9Async(host.Services, context, token),
            RunTimekeeping11Async(host.Services, context, token),
            RunWorkplaces13Async(host.Services, context, token));
    }
    private static async Task RunCorporateevents1Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiCorporateevents.CorporateEventReportingService.CorporateEventReportingServiceClient>();
        var writer = provider.GetRequiredService<ApiCorporateevents.CorporateEventService.CorporateEventServiceClient>();
        var request = new ApiCorporateevents.StreamCorporateEventMetricsRequest
        {
            Context = context,
        };
        using var stream = reader.StreamCorporateEventMetrics(request, cancellationToken: token);
        await foreach (var change in stream.ResponseStream.ReadAllAsync(token))
        {
            if (change.CorporateEvent is not { } item)
                continue;
            using var update = writer.UpdateCorporateEventAsync(new ApiCorporateevents.UpdateCorporateEventRequest
            {
                Context = context,
                CorporateEvent = item.Clone(),
            }, cancellationToken: token);
            await update.ResponseAsync;
        }
    }
    private static async Task RunEmployees4Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiEmployees.EmployeeService.EmployeeServiceClient>();
        var writer = provider.GetRequiredService<ApiEmployees.EmployeeService.EmployeeServiceClient>();
        var request = new ApiEmployees.WatchEmployeesRequest
        {
            Context = context,
        };
        using var stream = reader.WatchEmployees(request, cancellationToken: token);
        await foreach (var change in stream.ResponseStream.ReadAllAsync(token))
        {
            if (change.Employee is not { } item)
                continue;
            using var update = writer.UpdateEmployeeAsync(new ApiEmployees.UpdateEmployeeRequest
            {
                Context = context,
                Employee = item.Clone(),
            }, cancellationToken: token);
            await update.ResponseAsync;
        }
    }
    private static async Task RunPerformance9Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiPerformance.ReviewService.ReviewServiceClient>();
        var writer = provider.GetRequiredService<ApiPerformance.ReviewService.ReviewServiceClient>();
        var request = new ApiPerformance.WatchReviewsRequest
        {
            Context = context,
        };
        using var stream = reader.WatchReviews(request, cancellationToken: token);
        await foreach (var change in stream.ResponseStream.ReadAllAsync(token))
        {
            if (change.Review is not { } item)
                continue;
            using var update = writer.UpdateReviewAsync(new ApiPerformance.UpdateReviewRequest
            {
                Context = context,
                Review = item.Clone(),
            }, cancellationToken: token);
            await update.ResponseAsync;
        }
    }
    private static async Task RunTimekeeping11Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiTimekeeping.TimesheetService.TimesheetServiceClient>();
        var writer = provider.GetRequiredService<ApiTimekeeping.TimesheetService.TimesheetServiceClient>();
        var request = new ApiTimekeeping.WatchTimesheetsRequest
        {
            Context = context,
        };
        using var stream = reader.WatchTimesheets(request, cancellationToken: token);
        await foreach (var change in stream.ResponseStream.ReadAllAsync(token))
        {
            if (change.Timesheet is not { } item)
                continue;
            using var update = writer.UpdateTimesheetAsync(new ApiTimekeeping.UpdateTimesheetRequest
            {
                Context = context,
                Timesheet = item.Clone(),
            }, cancellationToken: token);
            await update.ResponseAsync;
        }
    }
    private static async Task RunWorkplaces13Async(IServiceProvider provider, ApiCommon.RequestContext context, CancellationToken token)
    {
        var reader = provider.GetRequiredService<ApiWorkplaces.BuildingService.BuildingServiceClient>();
        var writer = provider.GetRequiredService<ApiWorkplaces.BuildingService.BuildingServiceClient>();
        var request = new ApiWorkplaces.WatchBuildingsRequest
        {
            Context = context,
        };
        using var stream = reader.WatchBuildings(request, cancellationToken: token);
        await foreach (var change in stream.ResponseStream.ReadAllAsync(token))
        {
            if (change.Building is not { } item)
                continue;
            using var update = writer.UpdateBuildingAsync(new ApiWorkplaces.UpdateBuildingRequest
            {
                Building = item.Clone(),
                Context = context,
            }, cancellationToken: token);
            await update.ResponseAsync;
        }
    }
}
