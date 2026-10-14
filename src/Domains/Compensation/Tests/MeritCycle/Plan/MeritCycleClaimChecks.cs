namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleClaimChecks
{
    public static void MeritCycleClaimStartsInPlanned()
    {
        var item = MeritCycleClaim.NewMeritCycleClaim("code-211", "title-330");
        Check.Equal(item.Status, MeritCycleClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleClaimMovesToCompleted()
    {
        var item = MeritCycleClaim.NewMeritCycleClaim("code-211", "title-330");
        try
        {
            item.MoveTo(MeritCycleClaimStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleClaimStatus.Completed, "status");
    }

    public static void MeritCycleClaimRejectsMovesFromCancelled()
    {
        var item = MeritCycleClaim.NewMeritCycleClaim("code-211", "title-330");
        item.Status = MeritCycleClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(MeritCycleClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        MeritCycleClaimStartsInPlanned();
        MeritCycleClaimMovesToCompleted();
        MeritCycleClaimRejectsMovesFromCancelled();
    }
}
