namespace Acme.Workforce.Orgdesign.Tests.Position.Policy;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PositionPolicyViewChecks
{
    public static void PositionPolicyViewShowsTheStatusInCapitals()
    {
        var item = PositionPolicy.NewPositionPolicy("code-840", "label-164");
        var view = PositionPolicyView.ToPositionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionPolicyView.ToPositionPolicyViews(new List<PositionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionPolicyViewShowsTheStatusInCapitals();
    }
}
