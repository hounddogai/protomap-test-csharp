namespace Acme.Workforce.Compensation.Tests.MeritCycle.Batch;

using Acme.Workforce.Compensation.MeritCycle.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingMeritCycleBatchViewChecks
{
    public static void PendingMeritCycleBatchViewShowsTheStatusInCapitals()
    {
        var item = PendingMeritCycleBatch.NewPendingMeritCycleBatch("code-524", "title-208");
        var view = PendingMeritCycleBatchView.ToPendingMeritCycleBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingMeritCycleBatchView.ToPendingMeritCycleBatchViews(new List<PendingMeritCycleBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingMeritCycleBatchViewShowsTheStatusInCapitals();
    }
}
