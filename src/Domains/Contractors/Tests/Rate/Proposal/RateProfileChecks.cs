namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RateProfileChecks
{
    public static void RateProfileStartsInNew()
    {
        var item = RateProfile.NewRateProfile("code-138", "title-450");
        Check.Equal(item.Status, RateProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateProfileMovesToQueued()
    {
        var item = RateProfile.NewRateProfile("code-138", "title-450");
        try
        {
            item.MoveTo(RateProfileStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateProfileStatus.Queued, "status");
    }

    public static void RateProfileRejectsMovesFromFailed()
    {
        var item = RateProfile.NewRateProfile("code-138", "title-450");
        item.Status = RateProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(RateProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        RateProfileStartsInNew();
        RateProfileMovesToQueued();
        RateProfileRejectsMovesFromFailed();
    }
}
