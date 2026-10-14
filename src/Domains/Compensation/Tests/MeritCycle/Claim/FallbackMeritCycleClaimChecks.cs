namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class FallbackMeritCycleClaimChecks
{
    public static void FallbackMeritCycleClaimStartsInDraft()
    {
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("code-427", "label-592");
        Check.Equal(item.Status, FallbackMeritCycleClaimStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackMeritCycleClaimMovesToPending()
    {
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("code-427", "label-592");
        try
        {
            item.MoveTo(FallbackMeritCycleClaimStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackMeritCycleClaimStatus.Pending, "status");
    }

    public static void FallbackMeritCycleClaimRejectsMovesFromClosed()
    {
        var item = FallbackMeritCycleClaim.NewFallbackMeritCycleClaim("code-427", "label-592");
        item.Status = FallbackMeritCycleClaimStatus.Closed;
        Check.Fails(() => item.MoveTo(FallbackMeritCycleClaimStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FallbackMeritCycleClaimStartsInDraft();
        FallbackMeritCycleClaimMovesToPending();
        FallbackMeritCycleClaimRejectsMovesFromClosed();
    }
}
