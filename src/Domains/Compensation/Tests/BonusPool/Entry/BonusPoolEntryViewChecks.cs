namespace Acme.Workforce.Compensation.Tests.BonusPool.Entry;

using Acme.Workforce.Compensation.BonusPool.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BonusPoolEntryViewChecks
{
    public static void BonusPoolEntryViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolEntry.NewBonusPoolEntry("code-279", "name-136");
        var view = BonusPoolEntryView.ToBonusPoolEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolEntryView.ToBonusPoolEntryViews(new List<BonusPoolEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolEntryViewShowsTheStatusInCapitals();
    }
}
