namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyItemViewChecks
{
    public static void PolicyItemViewShowsTheStatusInCapitals()
    {
        var item = PolicyItem.NewPolicyItem("code-60", "label-718");
        var view = PolicyItemView.ToPolicyItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PolicyItemView.ToPolicyItemViews(new List<PolicyItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PolicyItemViewShowsTheStatusInCapitals();
    }
}
