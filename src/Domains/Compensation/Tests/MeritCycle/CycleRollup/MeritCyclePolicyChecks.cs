namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class MeritCyclePolicyChecks
{
    public static void MeritCyclePolicyStartsInPlanned()
    {
        var item = MeritCyclePolicy.NewMeritCyclePolicy("code-385", "label-130");
        Check.Equal(item.Status, MeritCyclePolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCyclePolicyMovesToInProgress()
    {
        var item = MeritCyclePolicy.NewMeritCyclePolicy("code-385", "label-130");
        try
        {
            item.MoveTo(MeritCyclePolicyStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCyclePolicyStatus.InProgress, "status");
    }

    public static void MeritCyclePolicyRejectsMovesFromCancelled()
    {
        var item = MeritCyclePolicy.NewMeritCyclePolicy("code-385", "label-130");
        item.Status = MeritCyclePolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MeritCyclePolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MeritCyclePolicyStartsInPlanned();
        MeritCyclePolicyMovesToInProgress();
        MeritCyclePolicyRejectsMovesFromCancelled();
    }
}
