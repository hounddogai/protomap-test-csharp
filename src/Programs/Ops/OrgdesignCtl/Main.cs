namespace Acme.Workforce.Ops.OrgdesignCtl;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Orgdesign.CostModel.Score;
using Acme.Workforce.Orgdesign.Position.Mapping;
using Acme.Workforce.Orgdesign.Span.Slot;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["position-tier"] = () =>
            {
                var store = PositionTierStore.NewMemoryPositionTierStore();
                for (var index = 0; index < 9; index++)
                {
                    store.Save(PositionTier.NewPositionTier($"position-tier-{index}", $"Sample position tier {index}"));
                }
                var flow = PositionTierWorkflow.NewPositionTierDesk(store, clock);
                try
                {
                    flow.Start("position-tier-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = PositionTierReport.BuildPositionTierReport("Position Tier", items, clock);
                Console.WriteLine(report.Render());
                return flow.Audit((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["span-override"] = () =>
            {
                var store = SpanOverrideStore.NewMemorySpanOverrideStore();
                for (var index = 0; index < 8; index++)
                {
                    store.Save(SpanOverride.NewSpanOverride($"span-override-{index}", $"Sample span override {index}"));
                }
                var flow = SpanOverrideWorkflow.NewSpanOverrideWorkflow(store, clock);
                try
                {
                    flow.Start("span-override-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = SpanOverrideReport.BuildSpanOverrideReport("Span Override", items, clock);
                Console.WriteLine(report.Render());
                return flow.Audit((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["cost-model-slot"] = () =>
            {
                var store = CostModelSlotStore.NewMemoryCostModelSlotStore();
                for (var index = 0; index < 12; index++)
                {
                    store.Save(CostModelSlot.NewCostModelSlot($"cost-model-slot-{index}", $"Sample cost model slot {index}"));
                }
                var flow = CostModelSlotWorkflow.NewCostModelSlotWorkflow(store, clock);
                try
                {
                    flow.Open("cost-model-slot-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = CostModelSlotReport.BuildCostModelSlotReport("Cost Model Slot", items, clock);
                Console.WriteLine(report.Render());
                return flow.Audit((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
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
