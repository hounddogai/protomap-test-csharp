namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedTownHallExceptionViewChecks
{
    public static void BatchedTownHallExceptionViewShowsTheStatusInCapitals()
    {
        var item = BatchedTownHallException.NewBatchedTownHallException("code-424", "name-187");
        var view = BatchedTownHallExceptionView.ToBatchedTownHallExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedTownHallExceptionView.ToBatchedTownHallExceptionViews(new List<BatchedTownHallException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedTownHallExceptionViewShowsTheStatusInCapitals();
    }
}
