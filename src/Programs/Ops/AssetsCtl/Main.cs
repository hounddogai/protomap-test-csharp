namespace Acme.Workforce.Ops.AssetsCtl;

using Acme.Workforce.Assets.AssetReturn.Reservation;
using Acme.Workforce.Assets.Laptop.Milestone;
using Acme.Workforce.Assets.Refresh.Metric;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["laptop-checkpoint"] = () =>
            {
                var store = LaptopCheckpointStore.NewMemoryLaptopCheckpointStore();
                for (var index = 0; index < 9; index++)
                {
                    store.Save(LaptopCheckpoint.NewLaptopCheckpoint($"laptop-checkpoint-{index}", $"Sample laptop checkpoint {index}"));
                }
                var flow = LaptopCheckpointWorkflow.NewLaptopCheckpointWorkflow(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Open("laptop-checkpoint-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = LaptopCheckpointReport.BuildLaptopCheckpointReport("Laptop Checkpoint", items, clock);
                Console.WriteLine(report.Render());
                return flow.Scan((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["refresh-cycle"] = () =>
            {
                var store = RefreshCycleStore.NewMemoryRefreshCycleStore();
                for (var index = 0; index < 12; index++)
                {
                    store.Save(RefreshCycle.NewRefreshCycle($"refresh-cycle-{index}", $"Sample refresh cycle {index}"));
                }
                var flow = RefreshCycleWorkflow.NewRefreshCycleManager(store, clock);
                try
                {
                    flow.Submit("refresh-cycle-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = RefreshCycleReport.BuildRefreshCycleReport("Refresh Cycle", items, clock);
                Console.WriteLine(report.Render());
                return flow.Audit((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["asset-return-reservation"] = () =>
            {
                var store = AssetReturnReservationStore.NewMemoryAssetReturnReservationStore();
                for (var index = 0; index < 11; index++)
                {
                    store.Save(AssetReturnReservation.NewAssetReturnReservation($"asset-return-reservation-{index}", $"Sample asset return reservation {index}"));
                }
                var flow = AssetReturnReservationWorkflow.NewAssetReturnReservationManager(store, clock);
                try
                {
                    flow.Start("asset-return-reservation-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = AssetReturnReservationReport.BuildAssetReturnReservationReport("Asset Return Reservation", items, clock);
                Console.WriteLine(report.Render());
                return flow.Scan((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
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
