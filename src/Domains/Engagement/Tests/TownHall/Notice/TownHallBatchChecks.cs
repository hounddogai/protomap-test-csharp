namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallBatchChecks
{
    public static void TownHallBatchStartsInCreated()
    {
        var item = TownHallBatch.NewTownHallBatch("code-474", "title-779");
        Check.Equal(item.Status, TownHallBatchStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallBatchMovesToValidated()
    {
        var item = TownHallBatch.NewTownHallBatch("code-474", "title-779");
        try
        {
            item.MoveTo(TownHallBatchStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallBatchStatus.Validated, "status");
    }

    public static void TownHallBatchRejectsMovesFromRetired()
    {
        var item = TownHallBatch.NewTownHallBatch("code-474", "title-779");
        item.Status = TownHallBatchStatus.Retired;
        Check.Fails(() => item.MoveTo(TownHallBatchStatus.Created), "move");
    }

    public static void RunAll()
    {
        TownHallBatchStartsInCreated();
        TownHallBatchMovesToValidated();
        TownHallBatchRejectsMovesFromRetired();
    }
}
