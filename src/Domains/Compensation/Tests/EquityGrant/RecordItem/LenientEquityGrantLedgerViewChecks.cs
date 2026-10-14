namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientEquityGrantLedgerViewChecks
{
    public static void LenientEquityGrantLedgerViewShowsTheStatusInCapitals()
    {
        var item = LenientEquityGrantLedger.NewLenientEquityGrantLedger("code-416", "title-25");
        var view = LenientEquityGrantLedgerView.ToLenientEquityGrantLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientEquityGrantLedgerView.ToLenientEquityGrantLedgerViews(new List<LenientEquityGrantLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientEquityGrantLedgerViewShowsTheStatusInCapitals();
    }
}
