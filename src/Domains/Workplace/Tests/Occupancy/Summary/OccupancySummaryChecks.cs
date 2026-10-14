namespace Acme.Workforce.Workplace.Tests.Occupancy.Summary;

using Acme.Workforce.Workplace.Occupancy.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class OccupancySummaryChecks
{
    public static void OccupancySummaryStartsInRequested()
    {
        var item = OccupancySummary.NewOccupancySummary("code-629", "label-802");
        Check.Equal(item.Status, OccupancySummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancySummaryMovesToRevoked()
    {
        var item = OccupancySummary.NewOccupancySummary("code-629", "label-802");
        try
        {
            item.MoveTo(OccupancySummaryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancySummaryStatus.Revoked, "status");
    }

    public static void OccupancySummaryRejectsMovesFromExpired()
    {
        var item = OccupancySummary.NewOccupancySummary("code-629", "label-802");
        item.Status = OccupancySummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(OccupancySummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        OccupancySummaryStartsInRequested();
        OccupancySummaryMovesToRevoked();
        OccupancySummaryRejectsMovesFromExpired();
    }
}
