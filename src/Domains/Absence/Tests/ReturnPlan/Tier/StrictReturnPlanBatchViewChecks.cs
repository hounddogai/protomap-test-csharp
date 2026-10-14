namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictReturnPlanBatchViewChecks
{
    public static void StrictReturnPlanBatchViewShowsTheStatusInCapitals()
    {
        var item = StrictReturnPlanBatch.NewStrictReturnPlanBatch("code-608", "title-977");
        var view = StrictReturnPlanBatchView.ToStrictReturnPlanBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictReturnPlanBatchView.ToStrictReturnPlanBatchViews(new List<StrictReturnPlanBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictReturnPlanBatchViewShowsTheStatusInCapitals();
    }
}
