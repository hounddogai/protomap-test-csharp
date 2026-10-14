namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class WeightedVisitorBaselineViewChecks
{
    public static void WeightedVisitorBaselineViewShowsTheStatusInCapitals()
    {
        var item = WeightedVisitorBaseline.NewWeightedVisitorBaseline("code-890", "label-845");
        var view = WeightedVisitorBaselineView.ToWeightedVisitorBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedVisitorBaselineView.ToWeightedVisitorBaselineViews(new List<WeightedVisitorBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedVisitorBaselineViewShowsTheStatusInCapitals();
    }
}
