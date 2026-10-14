namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelEntryViewChecks
{
    public static void CostModelEntryViewShowsTheStatusInCapitals()
    {
        var item = CostModelEntry.NewCostModelEntry("code-334", "name-143");
        var view = CostModelEntryView.ToCostModelEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelEntryView.ToCostModelEntryViews(new List<CostModelEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelEntryViewShowsTheStatusInCapitals();
    }
}
