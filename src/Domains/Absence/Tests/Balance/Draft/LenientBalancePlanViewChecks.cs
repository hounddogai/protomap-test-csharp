namespace Acme.Workforce.Absence.Tests.Balance.Draft;

using Acme.Workforce.Absence.Balance.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientBalancePlanViewChecks
{
    public static void LenientBalancePlanViewShowsTheStatusInCapitals()
    {
        var item = LenientBalancePlan.NewLenientBalancePlan("code-461", "name-118");
        var view = LenientBalancePlanView.ToLenientBalancePlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBalancePlanView.ToLenientBalancePlanViews(new List<LenientBalancePlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBalancePlanViewShowsTheStatusInCapitals();
    }
}
