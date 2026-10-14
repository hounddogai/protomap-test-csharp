namespace Acme.Workforce.Absence.Tests.ReturnPlan.Preference;

using Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ReturnPlanLedgerViewChecks
{
    public static void ReturnPlanLedgerViewShowsTheStatusInCapitals()
    {
        var item = ReturnPlanLedger.NewReturnPlanLedger("code-792", "title-100");
        var view = ReturnPlanLedgerView.ToReturnPlanLedgerView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReturnPlanLedgerView.ToReturnPlanLedgerViews(new List<ReturnPlanLedger> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReturnPlanLedgerViewShowsTheStatusInCapitals();
    }
}
