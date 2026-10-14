namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationProfileChecks
{
    public static void NominationProfileStartsInOpen()
    {
        var item = NominationProfile.NewNominationProfile("code-503", "title-872");
        Check.Equal(item.Status, NominationProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void NominationProfileMovesToReopened()
    {
        var item = NominationProfile.NewNominationProfile("code-503", "title-872");
        try
        {
            item.MoveTo(NominationProfileStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, NominationProfileStatus.Reopened, "status");
    }

    public static void NominationProfileRejectsMovesFromArchived()
    {
        var item = NominationProfile.NewNominationProfile("code-503", "title-872");
        item.Status = NominationProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(NominationProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        NominationProfileStartsInOpen();
        NominationProfileMovesToReopened();
        NominationProfileRejectsMovesFromArchived();
    }
}
