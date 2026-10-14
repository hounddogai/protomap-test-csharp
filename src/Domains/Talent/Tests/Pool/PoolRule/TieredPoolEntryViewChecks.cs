namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class TieredPoolEntryViewChecks
{
    public static void TieredPoolEntryViewShowsTheStatusInCapitals()
    {
        var item = TieredPoolEntry.NewTieredPoolEntry("code-746", "name-262");
        var view = TieredPoolEntryView.ToTieredPoolEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPoolEntryView.ToTieredPoolEntryViews(new List<TieredPoolEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPoolEntryViewShowsTheStatusInCapitals();
    }
}
