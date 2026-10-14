namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class StrictPayBandDraftChecks
{
    public static void StrictPayBandDraftStartsInOpen()
    {
        var item = StrictPayBandDraft.NewStrictPayBandDraft("code-622", "name-946");
        Check.Equal(item.Status, StrictPayBandDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPayBandDraftMovesToReopened()
    {
        var item = StrictPayBandDraft.NewStrictPayBandDraft("code-622", "name-946");
        try
        {
            item.MoveTo(StrictPayBandDraftStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPayBandDraftStatus.Reopened, "status");
    }

    public static void StrictPayBandDraftRejectsMovesFromArchived()
    {
        var item = StrictPayBandDraft.NewStrictPayBandDraft("code-622", "name-946");
        item.Status = StrictPayBandDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictPayBandDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictPayBandDraftStartsInOpen();
        StrictPayBandDraftMovesToReopened();
        StrictPayBandDraftRejectsMovesFromArchived();
    }
}
