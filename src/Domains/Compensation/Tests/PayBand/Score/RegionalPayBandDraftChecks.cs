namespace Acme.Workforce.Compensation.Tests.PayBand.Score;

using Acme.Workforce.Compensation.PayBand.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class RegionalPayBandDraftChecks
{
    public static void RegionalPayBandDraftStartsInRequested()
    {
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("code-971", "title-999");
        Check.Equal(item.Status, RegionalPayBandDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalPayBandDraftMovesToGranted()
    {
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("code-971", "title-999");
        try
        {
            item.MoveTo(RegionalPayBandDraftStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalPayBandDraftStatus.Granted, "status");
    }

    public static void RegionalPayBandDraftRejectsMovesFromExpired()
    {
        var item = RegionalPayBandDraft.NewRegionalPayBandDraft("code-971", "title-999");
        item.Status = RegionalPayBandDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(RegionalPayBandDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RegionalPayBandDraftStartsInRequested();
        RegionalPayBandDraftMovesToGranted();
        RegionalPayBandDraftRejectsMovesFromExpired();
    }
}
