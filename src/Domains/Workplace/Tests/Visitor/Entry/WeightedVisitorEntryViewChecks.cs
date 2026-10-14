namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedVisitorEntryViewChecks
{
    public static void WeightedVisitorEntryViewShowsTheStatusInCapitals()
    {
        var item = WeightedVisitorEntry.NewWeightedVisitorEntry("code-953", "name-789");
        var view = WeightedVisitorEntryView.ToWeightedVisitorEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedVisitorEntryView.ToWeightedVisitorEntryViews(new List<WeightedVisitorEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedVisitorEntryViewShowsTheStatusInCapitals();
    }
}
