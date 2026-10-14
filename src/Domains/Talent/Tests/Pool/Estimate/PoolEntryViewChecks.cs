namespace Acme.Workforce.Talent.Tests.Pool.Estimate;

using Acme.Workforce.Talent.Pool.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PoolEntryViewChecks
{
    public static void PoolEntryViewShowsTheStatusInCapitals()
    {
        var item = PoolEntry.NewPoolEntry("code-808", "name-388");
        var view = PoolEntryView.ToPoolEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolEntryView.ToPoolEntryViews(new List<PoolEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolEntryViewShowsTheStatusInCapitals();
    }
}
