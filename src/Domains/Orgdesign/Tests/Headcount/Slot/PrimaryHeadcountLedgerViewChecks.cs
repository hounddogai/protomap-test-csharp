namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class PrimaryHeadcountLedgerViewChecks
{
    public static void PrimaryHeadcountLedgerViewShowsTheStatusInCapitals()
    {
        var item = PrimaryHeadcountLedger.NewPrimaryHeadcountLedger("code-124", "title-19");
        var view = PrimaryHeadcountLedgerView.ToPrimaryHeadcountLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryHeadcountLedgerView.ToPrimaryHeadcountLedgerViews(new List<PrimaryHeadcountLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryHeadcountLedgerViewShowsTheStatusInCapitals();
    }
}
