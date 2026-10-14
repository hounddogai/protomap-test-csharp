namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSummaryChecks
{
    public static void LicenseSeatSummaryStartsInRequested()
    {
        var item = LicenseSeatSummary.NewLicenseSeatSummary("code-189", "title-520");
        Check.Equal(item.Status, LicenseSeatSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatSummaryMovesToGranted()
    {
        var item = LicenseSeatSummary.NewLicenseSeatSummary("code-189", "title-520");
        try
        {
            item.MoveTo(LicenseSeatSummaryStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatSummaryStatus.Granted, "status");
    }

    public static void LicenseSeatSummaryRejectsMovesFromExpired()
    {
        var item = LicenseSeatSummary.NewLicenseSeatSummary("code-189", "title-520");
        item.Status = LicenseSeatSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(LicenseSeatSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LicenseSeatSummaryStartsInRequested();
        LicenseSeatSummaryMovesToGranted();
        LicenseSeatSummaryRejectsMovesFromExpired();
    }
}
