namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleItemChecks
{
    public static void MeritCycleItemStartsInPlanned()
    {
        var item = MeritCycleItem.NewMeritCycleItem("code-128", "label-991");
        Check.Equal(item.Status, MeritCycleItemStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleItemMovesToScheduled()
    {
        var item = MeritCycleItem.NewMeritCycleItem("code-128", "label-991");
        try
        {
            item.MoveTo(MeritCycleItemStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleItemStatus.Scheduled, "status");
    }

    public static void MeritCycleItemRejectsMovesFromCancelled()
    {
        var item = MeritCycleItem.NewMeritCycleItem("code-128", "label-991");
        item.Status = MeritCycleItemStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MeritCycleItemStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MeritCycleItemStartsInPlanned();
        MeritCycleItemMovesToScheduled();
        MeritCycleItemRejectsMovesFromCancelled();
    }
}
