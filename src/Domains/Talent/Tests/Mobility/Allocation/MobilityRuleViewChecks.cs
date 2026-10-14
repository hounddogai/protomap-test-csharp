namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MobilityRuleViewChecks
{
    public static void MobilityRuleViewShowsTheStatusInCapitals()
    {
        var item = MobilityRule.NewMobilityRule("code-802", "name-35");
        var view = MobilityRuleView.ToMobilityRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityRuleView.ToMobilityRuleViews(new List<MobilityRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityRuleViewShowsTheStatusInCapitals();
    }
}
