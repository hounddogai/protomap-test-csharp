namespace Acme.Workforce.Ops.PayrollopsCtl;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Baseline;
using Acme.Workforce.Payrollops.Garnishment.Plan;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["deduction-notice"] = () =>
            {
                var store = DeductionNoticeStore.NewMemoryDeductionNoticeStore();
                for (var index = 0; index < 9; index++)
                {
                    store.Save(DeductionNotice.NewDeductionNotice($"deduction-notice-{index}", $"Sample deduction notice {index}"));
                }
                var flow = DeductionNoticeWorkflow.NewDeductionNoticeCoordinator(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Submit("deduction-notice-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = DeductionNoticeReport.BuildDeductionNoticeReport("Deduction Notice", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["garnishment-metric"] = () =>
            {
                var store = GarnishmentMetricStore.NewMemoryGarnishmentMetricStore();
                for (var index = 0; index < 3; index++)
                {
                    store.Save(GarnishmentMetric.NewGarnishmentMetric($"garnishment-metric-{index}", $"Sample garnishment metric {index}"));
                }
                var flow = GarnishmentMetricWorkflow.NewGarnishmentMetricDesk(store, clock);
                try
                {
                    flow.Start("garnishment-metric-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = GarnishmentMetricReport.BuildGarnishmentMetricReport("Garnishment Metric", items, clock);
                Console.WriteLine(report.Render());
                return flow.Sweep((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
        };
        var failed = 0;
        foreach (var name in commands.Keys.Order(StringComparer.Ordinal))
        {
            var command = commands[name];
            if (!command())
            {
                Console.WriteLine($"{name} failed");
                failed += 1;
            }
        }
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }
}
