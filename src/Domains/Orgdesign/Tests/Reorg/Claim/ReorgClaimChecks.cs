namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgClaimChecks
{
    public static void ReorgClaimStartsInPlanned()
    {
        var item = ReorgClaim.NewReorgClaim("code-906", "title-918");
        Check.Equal(item.Status, ReorgClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgClaimMovesToScheduled()
    {
        var item = ReorgClaim.NewReorgClaim("code-906", "title-918");
        try
        {
            item.MoveTo(ReorgClaimStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgClaimStatus.Scheduled, "status");
    }

    public static void ReorgClaimRejectsMovesFromCancelled()
    {
        var item = ReorgClaim.NewReorgClaim("code-906", "title-918");
        item.Status = ReorgClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ReorgClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ReorgClaimStartsInPlanned();
        ReorgClaimMovesToScheduled();
        ReorgClaimRejectsMovesFromCancelled();
    }
}
