namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredEquityGrantLedgerViewChecks
{
    public static void TieredEquityGrantLedgerViewShowsTheStatusInCapitals()
    {
        var item = TieredEquityGrantLedger.NewTieredEquityGrantLedger("code-120", "name-257");
        var view = TieredEquityGrantLedgerView.ToTieredEquityGrantLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredEquityGrantLedgerView.ToTieredEquityGrantLedgerViews(new List<TieredEquityGrantLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredEquityGrantLedgerViewShowsTheStatusInCapitals();
    }
}
