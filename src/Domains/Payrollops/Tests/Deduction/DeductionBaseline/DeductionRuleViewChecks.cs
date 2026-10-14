namespace Acme.Workforce.Payrollops.Tests.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

/// <summary>Checks of deduction baseline code, run by the project's runner.</summary>
public static class DeductionRuleViewChecks
{
    public static void DeductionRuleViewShowsTheStatusInCapitals()
    {
        var item = DeductionRule.NewDeductionRule("code-436", "name-264");
        var view = DeductionRuleView.ToDeductionRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionRuleView.ToDeductionRuleViews(new List<DeductionRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionRuleViewShowsTheStatusInCapitals();
    }
}
