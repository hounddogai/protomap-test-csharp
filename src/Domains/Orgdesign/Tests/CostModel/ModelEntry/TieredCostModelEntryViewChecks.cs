namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelEntry;

using Acme.Workforce.Orgdesign.CostModel.ModelEntry;

/// <summary>Checks of model entry code, run by the project's runner.</summary>
public static class TieredCostModelEntryViewChecks
{
    public static void TieredCostModelEntryViewShowsTheStatusInCapitals()
    {
        var item = TieredCostModelEntry.NewTieredCostModelEntry("code-336", "name-76");
        var view = TieredCostModelEntryView.ToTieredCostModelEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredCostModelEntryView.ToTieredCostModelEntryViews(new List<TieredCostModelEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredCostModelEntryViewShowsTheStatusInCapitals();
    }
}
