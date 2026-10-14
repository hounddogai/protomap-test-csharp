namespace Acme.Workforce.Orgdesign.Tests.Reorg.Baseline;

using Acme.Workforce.Orgdesign.Reorg.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ReorgBaselineChecks
{
    public static void ReorgBaselineStartsInRequested()
    {
        var item = ReorgBaseline.NewReorgBaseline("code-977", "title-547");
        Check.Equal(item.Status, ReorgBaselineStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgBaselineMovesToRevoked()
    {
        var item = ReorgBaseline.NewReorgBaseline("code-977", "title-547");
        try
        {
            item.MoveTo(ReorgBaselineStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgBaselineStatus.Revoked, "status");
    }

    public static void ReorgBaselineRejectsMovesFromExpired()
    {
        var item = ReorgBaseline.NewReorgBaseline("code-977", "title-547");
        item.Status = ReorgBaselineStatus.Expired;
        Check.Fails(() => item.MoveTo(ReorgBaselineStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ReorgBaselineStartsInRequested();
        ReorgBaselineMovesToRevoked();
        ReorgBaselineRejectsMovesFromExpired();
    }
}
