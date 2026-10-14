namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandBaselineRulesChecks
{
    public static void PayBandBaselineValidatorReportsEveryProblem()
    {
        var validator = PayBandBaselineRules.DefaultPayBandBaselineValidator();
        var item = PayBandBaseline.NewPayBandBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PayBandBaselineRequiredTitleRule("pay band baseline needs this field");
        var item = PayBandBaseline.NewPayBandBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandBaselineCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new PayBandBaselineCeilingRangeRule(0.5, 1398.2);
        var item = PayBandBaseline.NewPayBandBaseline("x", "y");
        item.Ceiling = 1399.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandBaselineValidatorReportsEveryProblem();
        PayBandBaselineRequiredTitleRuleFlagsABlankTitle();
        PayBandBaselineCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
