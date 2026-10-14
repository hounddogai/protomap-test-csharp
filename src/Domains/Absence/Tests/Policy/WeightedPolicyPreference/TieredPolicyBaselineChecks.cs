namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class TieredPolicyBaselineChecks
{
    public static void TieredPolicyBaselineStartsInRequested()
    {
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("code-785", "title-94");
        Check.Equal(item.Status, TieredPolicyBaselineStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPolicyBaselineMovesToGranted()
    {
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("code-785", "title-94");
        try
        {
            item.MoveTo(TieredPolicyBaselineStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPolicyBaselineStatus.Granted, "status");
    }

    public static void TieredPolicyBaselineRejectsMovesFromExpired()
    {
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("code-785", "title-94");
        item.Status = TieredPolicyBaselineStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredPolicyBaselineStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredPolicyBaselineStartsInRequested();
        TieredPolicyBaselineMovesToGranted();
        TieredPolicyBaselineRejectsMovesFromExpired();
    }
}
