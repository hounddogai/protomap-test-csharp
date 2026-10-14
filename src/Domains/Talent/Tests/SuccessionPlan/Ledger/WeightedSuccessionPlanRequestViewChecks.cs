namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Ledger;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class WeightedSuccessionPlanRequestViewChecks
{
    public static void WeightedSuccessionPlanRequestViewShowsTheStatusInCapitals()
    {
        var item = WeightedSuccessionPlanRequest.NewWeightedSuccessionPlanRequest("code-246", "label-390");
        var view = WeightedSuccessionPlanRequestView.ToWeightedSuccessionPlanRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedSuccessionPlanRequestView.ToWeightedSuccessionPlanRequestViews(new List<WeightedSuccessionPlanRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedSuccessionPlanRequestViewShowsTheStatusInCapitals();
    }
}
