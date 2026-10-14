namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

/// <summary>Checks of occupancy adjustment code, run by the project's runner.</summary>
public static class LegacyOccupancyExceptionChecks
{
    public static void LegacyOccupancyExceptionStartsInDraft()
    {
        var item = LegacyOccupancyException.NewLegacyOccupancyException("code-784", "title-249");
        Check.Equal(item.Status, LegacyOccupancyExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyOccupancyExceptionMovesToClosed()
    {
        var item = LegacyOccupancyException.NewLegacyOccupancyException("code-784", "title-249");
        try
        {
            item.MoveTo(LegacyOccupancyExceptionStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyOccupancyExceptionStatus.Closed, "status");
    }

    public static void LegacyOccupancyExceptionRejectsMovesFromClosed()
    {
        var item = LegacyOccupancyException.NewLegacyOccupancyException("code-784", "title-249");
        item.Status = LegacyOccupancyExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyOccupancyExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyOccupancyExceptionStartsInDraft();
        LegacyOccupancyExceptionMovesToClosed();
        LegacyOccupancyExceptionRejectsMovesFromClosed();
    }
}
