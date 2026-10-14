namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleLedgerViewChecks
{
    public static void MeritCycleLedgerViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleLedger.NewMeritCycleLedger("code-167", "label-616");
        var view = MeritCycleLedgerView.ToMeritCycleLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleLedgerView.ToMeritCycleLedgerViews(new List<MeritCycleLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleLedgerViewShowsTheStatusInCapitals();
    }
}
