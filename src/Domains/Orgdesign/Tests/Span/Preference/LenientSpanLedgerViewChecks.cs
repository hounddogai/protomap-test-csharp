namespace Acme.Workforce.Orgdesign.Tests.Span.Preference;

using Acme.Workforce.Orgdesign.Span.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientSpanLedgerViewChecks
{
    public static void LenientSpanLedgerViewShowsTheStatusInCapitals()
    {
        var item = LenientSpanLedger.NewLenientSpanLedger("code-158", "label-451");
        var view = LenientSpanLedgerView.ToLenientSpanLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSpanLedgerView.ToLenientSpanLedgerViews(new List<LenientSpanLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSpanLedgerViewShowsTheStatusInCapitals();
    }
}
