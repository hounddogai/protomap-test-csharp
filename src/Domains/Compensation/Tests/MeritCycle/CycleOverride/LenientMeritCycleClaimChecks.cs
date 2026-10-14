namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class LenientMeritCycleClaimChecks
{
    public static void LenientMeritCycleClaimStartsInOpen()
    {
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("code-620", "title-334");
        Check.Equal(item.Status, LenientMeritCycleClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientMeritCycleClaimMovesToReopened()
    {
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("code-620", "title-334");
        try
        {
            item.MoveTo(LenientMeritCycleClaimStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientMeritCycleClaimStatus.Reopened, "status");
    }

    public static void LenientMeritCycleClaimRejectsMovesFromArchived()
    {
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("code-620", "title-334");
        item.Status = LenientMeritCycleClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientMeritCycleClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientMeritCycleClaimStartsInOpen();
        LenientMeritCycleClaimMovesToReopened();
        LenientMeritCycleClaimRejectsMovesFromArchived();
    }
}
