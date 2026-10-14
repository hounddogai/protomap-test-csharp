namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualSegmentChecks
{
    public static void AccrualSegmentStartsInRequested()
    {
        var item = AccrualSegment.NewAccrualSegment("code-884", "name-632");
        Check.Equal(item.Status, AccrualSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualSegmentMovesToExpired()
    {
        var item = AccrualSegment.NewAccrualSegment("code-884", "name-632");
        try
        {
            item.MoveTo(AccrualSegmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualSegmentStatus.Expired, "status");
    }

    public static void AccrualSegmentRejectsMovesFromExpired()
    {
        var item = AccrualSegment.NewAccrualSegment("code-884", "name-632");
        item.Status = AccrualSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(AccrualSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AccrualSegmentStartsInRequested();
        AccrualSegmentMovesToExpired();
        AccrualSegmentRejectsMovesFromExpired();
    }
}
