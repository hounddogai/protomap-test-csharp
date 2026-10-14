namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class FallbackMobilityEntryViewChecks
{
    public static void FallbackMobilityEntryViewShowsTheStatusInCapitals()
    {
        var item = FallbackMobilityEntry.NewFallbackMobilityEntry("code-87", "title-682");
        var view = FallbackMobilityEntryView.ToFallbackMobilityEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackMobilityEntryView.ToFallbackMobilityEntryViews(new List<FallbackMobilityEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackMobilityEntryViewShowsTheStatusInCapitals();
    }
}
