namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RegionalNominationVarianceChecks
{
    public static void RegionalNominationVarianceStartsInCreated()
    {
        var item = RegionalNominationVariance.NewRegionalNominationVariance("code-915", "name-703");
        Check.Equal(item.Status, RegionalNominationVarianceStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalNominationVarianceMovesToPublished()
    {
        var item = RegionalNominationVariance.NewRegionalNominationVariance("code-915", "name-703");
        try
        {
            item.MoveTo(RegionalNominationVarianceStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalNominationVarianceStatus.Published, "status");
    }

    public static void RegionalNominationVarianceRejectsMovesFromRetired()
    {
        var item = RegionalNominationVariance.NewRegionalNominationVariance("code-915", "name-703");
        item.Status = RegionalNominationVarianceStatus.Retired;
        Check.Fails(() => item.MoveTo(RegionalNominationVarianceStatus.Created), "move");
    }

    public static void RunAll()
    {
        RegionalNominationVarianceStartsInCreated();
        RegionalNominationVarianceMovesToPublished();
        RegionalNominationVarianceRejectsMovesFromRetired();
    }
}
