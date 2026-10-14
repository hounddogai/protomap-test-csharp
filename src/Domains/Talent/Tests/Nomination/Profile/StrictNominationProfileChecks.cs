namespace Acme.Workforce.Talent.Tests.Nomination.Profile;

using Acme.Workforce.Talent.Nomination.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictNominationProfileChecks
{
    public static void StrictNominationProfileStartsInCreated()
    {
        var item = StrictNominationProfile.NewStrictNominationProfile("code-486", "name-627");
        Check.Equal(item.Status, StrictNominationProfileStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictNominationProfileMovesToValidated()
    {
        var item = StrictNominationProfile.NewStrictNominationProfile("code-486", "name-627");
        try
        {
            item.MoveTo(StrictNominationProfileStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictNominationProfileStatus.Validated, "status");
    }

    public static void StrictNominationProfileRejectsMovesFromRetired()
    {
        var item = StrictNominationProfile.NewStrictNominationProfile("code-486", "name-627");
        item.Status = StrictNominationProfileStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictNominationProfileStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictNominationProfileStartsInCreated();
        StrictNominationProfileMovesToValidated();
        StrictNominationProfileRejectsMovesFromRetired();
    }
}
