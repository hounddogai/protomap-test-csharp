namespace Acme.Workforce.Ops.CompensationCtl;

using Acme.Workforce.Compensation.EquityGrant.Summary;
using Acme.Workforce.Compensation.PayBand.Draft;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["pay-band-template"] = () =>
            {
                var store = PayBandTemplateStore.NewMemoryPayBandTemplateStore();
                for (var index = 0; index < 4; index++)
                {
                    store.Save(PayBandTemplate.NewPayBandTemplate($"pay-band-template-{index}", $"Sample pay band template {index}"));
                }
                var flow = PayBandTemplateWorkflow.NewPayBandTemplateCoordinator(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Start("pay-band-template-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = PayBandTemplateReport.BuildPayBandTemplateReport("Pay Band Template", items, clock);
                Console.WriteLine(report.Render());
                return flow.Sweep((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["equity-grant-summary"] = () =>
            {
                var store = EquityGrantSummaryStore.NewMemoryEquityGrantSummaryStore();
                for (var index = 0; index < 12; index++)
                {
                    store.Save(EquityGrantSummary.NewEquityGrantSummary($"equity-grant-summary-{index}", $"Sample equity grant summary {index}"));
                }
                var flow = EquityGrantSummaryWorkflow.NewEquityGrantSummaryManager(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Advance("equity-grant-summary-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = EquityGrantSummaryReport.BuildEquityGrantSummaryReport("Equity Grant Summary", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
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
