namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedNominationVarianceChecks
{
    public static void WeightedNominationVarianceStartsInOpen()
    {
        var item = WeightedNominationVariance.NewWeightedNominationVariance("code-706", "title-245");
        Check.Equal(item.Status, WeightedNominationVarianceStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedNominationVarianceMovesToInReview()
    {
        var item = WeightedNominationVariance.NewWeightedNominationVariance("code-706", "title-245");
        try
        {
            item.MoveTo(WeightedNominationVarianceStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedNominationVarianceStatus.InReview, "status");
    }

    public static void WeightedNominationVarianceRejectsMovesFromArchived()
    {
        var item = WeightedNominationVariance.NewWeightedNominationVariance("code-706", "title-245");
        item.Status = WeightedNominationVarianceStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedNominationVarianceStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedNominationVarianceStartsInOpen();
        WeightedNominationVarianceMovesToInReview();
        WeightedNominationVarianceRejectsMovesFromArchived();
    }
}
