namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalPositionPolicyViewChecks
{
    public static void RegionalPositionPolicyViewShowsTheStatusInCapitals()
    {
        var item = RegionalPositionPolicy.NewRegionalPositionPolicy("code-28", "label-225");
        var view = RegionalPositionPolicyView.ToRegionalPositionPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalPositionPolicyView.ToRegionalPositionPolicyViews(new List<RegionalPositionPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalPositionPolicyViewShowsTheStatusInCapitals();
    }
}
