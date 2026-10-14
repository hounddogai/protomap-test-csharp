namespace Acme.Workforce.Payrollops.Tests.Deduction.Draft;

using Acme.Workforce.Payrollops.Deduction.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class StrictDeductionRuleViewChecks
{
    public static void StrictDeductionRuleViewShowsTheStatusInCapitals()
    {
        var item = StrictDeductionRule.NewStrictDeductionRule("code-674", "label-252");
        var view = StrictDeductionRuleView.ToStrictDeductionRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictDeductionRuleView.ToStrictDeductionRuleViews(new List<StrictDeductionRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictDeductionRuleViewShowsTheStatusInCapitals();
    }
}
