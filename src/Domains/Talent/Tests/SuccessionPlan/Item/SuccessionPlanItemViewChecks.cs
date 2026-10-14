namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SuccessionPlanItemViewChecks
{
    public static void SuccessionPlanItemViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanItem.NewSuccessionPlanItem("code-790", "title-634");
        var view = SuccessionPlanItemView.ToSuccessionPlanItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanItemView.ToSuccessionPlanItemViews(new List<SuccessionPlanItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanItemViewShowsTheStatusInCapitals();
    }
}
