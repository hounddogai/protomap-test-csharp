namespace Acme.Workforce.Ops.AbsenceCtl;

using Acme.Workforce.Absence.Accrual.Cycle;
using Acme.Workforce.Absence.Balance.Group;
using Acme.Workforce.Absence.Holiday.Tier;
using Acme.Workforce.Absence.LeaveRequest.Item;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["leave-request-preference"] = () =>
            {
                var store = LeaveRequestPreferenceStore.NewMemoryLeaveRequestPreferenceStore();
                for (var index = 0; index < 7; index++)
                {
                    store.Save(LeaveRequestPreference.NewLeaveRequestPreference($"leave-request-preference-{index}", $"Sample leave request preference {index}"));
                }
                var flow = LeaveRequestPreferenceWorkflow.NewLeaveRequestPreferenceCoordinator(store, clock);
                try
                {
                    flow.Open("leave-request-preference-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = LeaveRequestPreferenceReport.BuildLeaveRequestPreferenceReport("Leave Request Preference", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["accrual-cycle"] = () =>
            {
                var store = AccrualCycleStore.NewMemoryAccrualCycleStore();
                for (var index = 0; index < 8; index++)
                {
                    store.Save(AccrualCycle.NewAccrualCycle($"accrual-cycle-{index}", $"Sample accrual cycle {index}"));
                }
                var flow = AccrualCycleWorkflow.NewAccrualCycleDesk(store, clock);
                try
                {
                    flow.Submit("accrual-cycle-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = AccrualCycleReport.BuildAccrualCycleReport("Accrual Cycle", items, clock);
                Console.WriteLine(report.Render());
                return flow.Scan((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["balance-group"] = () =>
            {
                var store = BalanceGroupStore.NewMemoryBalanceGroupStore();
                for (var index = 0; index < 8; index++)
                {
                    store.Save(BalanceGroup.NewBalanceGroup($"balance-group-{index}", $"Sample balance group {index}"));
                }
                var flow = BalanceGroupWorkflow.NewBalanceGroupCoordinator(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Submit("balance-group-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = BalanceGroupReport.BuildBalanceGroupReport("Balance Group", items, clock);
                Console.WriteLine(report.Render());
                return flow.Sweep((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["holiday-claim"] = () =>
            {
                var store = HolidayClaimStore.NewMemoryHolidayClaimStore();
                for (var index = 0; index < 6; index++)
                {
                    store.Save(HolidayClaim.NewHolidayClaim($"holiday-claim-{index}", $"Sample holiday claim {index}"));
                }
                var flow = HolidayClaimWorkflow.NewHolidayClaimCoordinator(store, clock);
                try
                {
                    flow.Open("holiday-claim-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = HolidayClaimReport.BuildHolidayClaimReport("Holiday Claim", items, clock);
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
