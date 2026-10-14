namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class WeightedRatePreferenceChecks
{
    public static void WeightedRatePreferenceStartsInProposed()
    {
        var item = WeightedRatePreference.NewWeightedRatePreference("code-521", "title-820");
        Check.Equal(item.Status, WeightedRatePreferenceStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRatePreferenceMovesToRejected()
    {
        var item = WeightedRatePreference.NewWeightedRatePreference("code-521", "title-820");
        try
        {
            item.MoveTo(WeightedRatePreferenceStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRatePreferenceStatus.Rejected, "status");
    }

    public static void WeightedRatePreferenceRejectsMovesFromWithdrawn()
    {
        var item = WeightedRatePreference.NewWeightedRatePreference("code-521", "title-820");
        item.Status = WeightedRatePreferenceStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedRatePreferenceStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedRatePreferenceStartsInProposed();
        WeightedRatePreferenceMovesToRejected();
        WeightedRatePreferenceRejectsMovesFromWithdrawn();
    }
}
