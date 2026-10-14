namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BalancePlanViewChecks
{
    public static void BalancePlanViewShowsTheStatusInCapitals()
    {
        var item = BalancePlan.NewBalancePlan("code-967", "title-559");
        var view = BalancePlanView.ToBalancePlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BalancePlanView.ToBalancePlanViews(new List<BalancePlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BalancePlanViewShowsTheStatusInCapitals();
    }
}
