namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class RegionalReorgPolicyViewChecks
{
    public static void RegionalReorgPolicyViewShowsTheStatusInCapitals()
    {
        var item = RegionalReorgPolicy.NewRegionalReorgPolicy("code-478", "name-516");
        var view = RegionalReorgPolicyView.ToRegionalReorgPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalReorgPolicyView.ToRegionalReorgPolicyViews(new List<RegionalReorgPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalReorgPolicyViewShowsTheStatusInCapitals();
    }
}
