namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleRuleChecks
{
    public static void MeritCycleRuleStartsInPlanned()
    {
        var item = MeritCycleRule.NewMeritCycleRule("code-410", "title-395");
        Check.Equal(item.Status, MeritCycleRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleRuleMovesToInProgress()
    {
        var item = MeritCycleRule.NewMeritCycleRule("code-410", "title-395");
        try
        {
            item.MoveTo(MeritCycleRuleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleRuleStatus.InProgress, "status");
    }

    public static void MeritCycleRuleRejectsMovesFromCancelled()
    {
        var item = MeritCycleRule.NewMeritCycleRule("code-410", "title-395");
        item.Status = MeritCycleRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MeritCycleRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MeritCycleRuleStartsInPlanned();
        MeritCycleRuleMovesToInProgress();
        MeritCycleRuleRejectsMovesFromCancelled();
    }
}
