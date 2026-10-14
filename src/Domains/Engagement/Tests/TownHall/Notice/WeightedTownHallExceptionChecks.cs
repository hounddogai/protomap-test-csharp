namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class WeightedTownHallExceptionChecks
{
    public static void WeightedTownHallExceptionStartsInPlanned()
    {
        var item = WeightedTownHallException.NewWeightedTownHallException("code-218", "name-921");
        Check.Equal(item.Status, WeightedTownHallExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedTownHallExceptionMovesToCompleted()
    {
        var item = WeightedTownHallException.NewWeightedTownHallException("code-218", "name-921");
        try
        {
            item.MoveTo(WeightedTownHallExceptionStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedTownHallExceptionStatus.Completed, "status");
    }

    public static void WeightedTownHallExceptionRejectsMovesFromCancelled()
    {
        var item = WeightedTownHallException.NewWeightedTownHallException("code-218", "name-921");
        item.Status = WeightedTownHallExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedTownHallExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedTownHallExceptionStartsInPlanned();
        WeightedTownHallExceptionMovesToCompleted();
        WeightedTownHallExceptionRejectsMovesFromCancelled();
    }
}
