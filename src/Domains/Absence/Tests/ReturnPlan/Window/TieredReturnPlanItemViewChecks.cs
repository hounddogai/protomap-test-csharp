namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class TieredReturnPlanItemViewChecks
{
    public static void TieredReturnPlanItemViewShowsTheStatusInCapitals()
    {
        var item = TieredReturnPlanItem.NewTieredReturnPlanItem("code-76", "title-175");
        var view = TieredReturnPlanItemView.ToTieredReturnPlanItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredReturnPlanItemView.ToTieredReturnPlanItemViews(new List<TieredReturnPlanItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredReturnPlanItemViewShowsTheStatusInCapitals();
    }
}
