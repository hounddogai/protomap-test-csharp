namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class DefaultPolicyItemViewChecks
{
    public static void DefaultPolicyItemViewShowsTheStatusInCapitals()
    {
        var item = DefaultPolicyItem.NewDefaultPolicyItem("code-7", "name-823");
        var view = DefaultPolicyItemView.ToDefaultPolicyItemView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultPolicyItemView.ToDefaultPolicyItemViews(new List<DefaultPolicyItem> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultPolicyItemViewShowsTheStatusInCapitals();
    }
}
