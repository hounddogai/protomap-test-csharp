namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAccessBadgeLedgerViewChecks
{
    public static void RegionalAccessBadgeLedgerViewShowsTheStatusInCapitals()
    {
        var item = RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger("code-78", "label-959");
        var view = RegionalAccessBadgeLedgerView.ToRegionalAccessBadgeLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalAccessBadgeLedgerView.ToRegionalAccessBadgeLedgerViews(new List<RegionalAccessBadgeLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeLedgerViewShowsTheStatusInCapitals();
    }
}
