namespace Acme.Workforce.Workplace.Tests.Desk.Digest;

using Acme.Workforce.Workplace.Desk.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class DeskLedgerViewChecks
{
    public static void DeskLedgerViewShowsTheStatusInCapitals()
    {
        var item = DeskLedger.NewDeskLedger("code-696", "title-260");
        var view = DeskLedgerView.ToDeskLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskLedgerView.ToDeskLedgerViews(new List<DeskLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskLedgerViewShowsTheStatusInCapitals();
    }
}
