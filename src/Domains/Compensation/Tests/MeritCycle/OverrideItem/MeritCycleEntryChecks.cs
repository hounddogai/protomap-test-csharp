namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleEntryChecks
{
    public static void MeritCycleEntryStartsInPlanned()
    {
        var item = MeritCycleEntry.NewMeritCycleEntry("code-190", "title-788");
        Check.Equal(item.Status, MeritCycleEntryStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleEntryMovesToScheduled()
    {
        var item = MeritCycleEntry.NewMeritCycleEntry("code-190", "title-788");
        try
        {
            item.MoveTo(MeritCycleEntryStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleEntryStatus.Scheduled, "status");
    }

    public static void MeritCycleEntryRejectsMovesFromCancelled()
    {
        var item = MeritCycleEntry.NewMeritCycleEntry("code-190", "title-788");
        item.Status = MeritCycleEntryStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MeritCycleEntryStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MeritCycleEntryStartsInPlanned();
        MeritCycleEntryMovesToScheduled();
        MeritCycleEntryRejectsMovesFromCancelled();
    }
}
