namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeLedgerViewChecks
{
    public static void AccessBadgeLedgerViewShowsTheStatusInCapitals()
    {
        var item = AccessBadgeLedger.NewAccessBadgeLedger("code-558", "label-682");
        var view = AccessBadgeLedgerView.ToAccessBadgeLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccessBadgeLedgerView.ToAccessBadgeLedgerViews(new List<AccessBadgeLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccessBadgeLedgerViewShowsTheStatusInCapitals();
    }
}
