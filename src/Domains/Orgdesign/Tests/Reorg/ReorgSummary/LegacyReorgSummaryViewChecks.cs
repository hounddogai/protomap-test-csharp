namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSummary;

using Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

/// <summary>Checks of reorg summary code, run by the project's runner.</summary>
public static class LegacyReorgSummaryViewChecks
{
    public static void LegacyReorgSummaryViewShowsTheStatusInCapitals()
    {
        var item = LegacyReorgSummary.NewLegacyReorgSummary("code-711", "title-16");
        var view = LegacyReorgSummaryView.ToLegacyReorgSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyReorgSummaryView.ToLegacyReorgSummaryViews(new List<LegacyReorgSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyReorgSummaryViewShowsTheStatusInCapitals();
    }
}
