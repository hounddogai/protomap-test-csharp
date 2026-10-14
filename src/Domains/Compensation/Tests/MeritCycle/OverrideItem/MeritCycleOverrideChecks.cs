namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleOverrideChecks
{
    public static void MeritCycleOverrideStartsInNew()
    {
        var item = MeritCycleOverride.NewMeritCycleOverride("code-517", "label-833");
        Check.Equal(item.Status, MeritCycleOverrideStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleOverrideMovesToFailed()
    {
        var item = MeritCycleOverride.NewMeritCycleOverride("code-517", "label-833");
        try
        {
            item.MoveTo(MeritCycleOverrideStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleOverrideStatus.Failed, "status");
    }

    public static void MeritCycleOverrideRejectsMovesFromFailed()
    {
        var item = MeritCycleOverride.NewMeritCycleOverride("code-517", "label-833");
        item.Status = MeritCycleOverrideStatus.Failed;
        Check.Fails(() => item.MoveTo(MeritCycleOverrideStatus.New), "move");
    }

    public static void RunAll()
    {
        MeritCycleOverrideStartsInNew();
        MeritCycleOverrideMovesToFailed();
        MeritCycleOverrideRejectsMovesFromFailed();
    }
}
