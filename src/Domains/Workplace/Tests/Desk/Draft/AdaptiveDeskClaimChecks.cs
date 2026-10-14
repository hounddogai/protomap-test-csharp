namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AdaptiveDeskClaimChecks
{
    public static void AdaptiveDeskClaimStartsInIdle()
    {
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("code-378", "label-486");
        Check.Equal(item.Status, AdaptiveDeskClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveDeskClaimMovesToReady()
    {
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("code-378", "label-486");
        try
        {
            item.MoveTo(AdaptiveDeskClaimStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveDeskClaimStatus.Ready, "status");
    }

    public static void AdaptiveDeskClaimRejectsMovesFromStopped()
    {
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("code-378", "label-486");
        item.Status = AdaptiveDeskClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveDeskClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveDeskClaimStartsInIdle();
        AdaptiveDeskClaimMovesToReady();
        AdaptiveDeskClaimRejectsMovesFromStopped();
    }
}
