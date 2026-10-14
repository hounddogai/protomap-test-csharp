namespace Acme.Workforce.Ops.WorkplaceCtl;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Variance;
using Acme.Workforce.Workplace.FloorPlan.Adjustment;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["booking-checkpoint"] = () =>
            {
                var store = BookingCheckpointStore.NewMemoryBookingCheckpointStore();
                for (var index = 0; index < 11; index++)
                {
                    store.Save(BookingCheckpoint.NewBookingCheckpoint($"booking-checkpoint-{index}", $"Sample booking checkpoint {index}"));
                }
                var flow = BookingCheckpointWorkflow.NewBookingCheckpointDesk(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Open("booking-checkpoint-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = BookingCheckpointReport.BuildBookingCheckpointReport("Booking Checkpoint", items, clock);
                Console.WriteLine(report.Render());
                return flow.Audit((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["floor-plan-record"] = () =>
            {
                var store = FloorPlanRecordStore.NewMemoryFloorPlanRecordStore();
                for (var index = 0; index < 7; index++)
                {
                    store.Save(FloorPlanRecord.NewFloorPlanRecord($"floor-plan-record-{index}", $"Sample floor plan record {index}"));
                }
                var flow = FloorPlanRecordWorkflow.NewFloorPlanRecordService(store, clock);
                try
                {
                    flow.Start("floor-plan-record-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = FloorPlanRecordReport.BuildFloorPlanRecordReport("Floor Plan Record", items, clock);
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
