namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSegmentChecks
{
    public static void LicenseSeatSegmentStartsInOpen()
    {
        var item = LicenseSeatSegment.NewLicenseSeatSegment("code-431", "label-701");
        Check.Equal(item.Status, LicenseSeatSegmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LicenseSeatSegmentMovesToReopened()
    {
        var item = LicenseSeatSegment.NewLicenseSeatSegment("code-431", "label-701");
        try
        {
            item.MoveTo(LicenseSeatSegmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LicenseSeatSegmentStatus.Reopened, "status");
    }

    public static void LicenseSeatSegmentRejectsMovesFromArchived()
    {
        var item = LicenseSeatSegment.NewLicenseSeatSegment("code-431", "label-701");
        item.Status = LicenseSeatSegmentStatus.Archived;
        Check.Fails(() => item.MoveTo(LicenseSeatSegmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        LicenseSeatSegmentStartsInOpen();
        LicenseSeatSegmentMovesToReopened();
        LicenseSeatSegmentRejectsMovesFromArchived();
    }
}
