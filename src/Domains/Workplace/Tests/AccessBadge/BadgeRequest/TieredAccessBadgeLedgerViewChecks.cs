namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class TieredAccessBadgeLedgerViewChecks
{
    public static void TieredAccessBadgeLedgerViewShowsTheStatusInCapitals()
    {
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("code-282", "label-376");
        var view = TieredAccessBadgeLedgerView.ToTieredAccessBadgeLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredAccessBadgeLedgerView.ToTieredAccessBadgeLedgerViews(new List<TieredAccessBadgeLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredAccessBadgeLedgerViewShowsTheStatusInCapitals();
    }
}
