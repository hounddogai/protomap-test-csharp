namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyAdjustmentViewChecks
{
    public static void PolicyAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = PolicyAdjustment.NewPolicyAdjustment("code-346", "title-519");
        var view = PolicyAdjustmentView.ToPolicyAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyAdjustmentView.ToPolicyAdjustmentViews(new List<PolicyAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyAdjustmentViewShowsTheStatusInCapitals();
    }
}
