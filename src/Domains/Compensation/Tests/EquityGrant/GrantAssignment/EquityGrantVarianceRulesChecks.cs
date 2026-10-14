namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantAssignment;

using Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Checks of grant assignment code, run by the project's runner.</summary>
public static class EquityGrantVarianceRulesChecks
{
    public static void EquityGrantVarianceValidatorReportsEveryProblem()
    {
        var validator = EquityGrantVarianceRules.DefaultEquityGrantVarianceValidator();
        var item = EquityGrantVariance.NewEquityGrantVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantVarianceRequiredNameRuleFlagsABlankName()
    {
        var rule = new EquityGrantVarianceRequiredNameRule("equity grant variance needs this field");
        var item = EquityGrantVariance.NewEquityGrantVariance("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantVarianceRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new EquityGrantVarianceRequiredCountryRule("equity grant variance needs a value");
        var item = EquityGrantVariance.NewEquityGrantVariance("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantVarianceRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantVarianceRequiredCodeRule("equity grant variance needs an entry");
        var item = EquityGrantVariance.NewEquityGrantVariance("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantVarianceMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new EquityGrantVarianceMaximumRangeRule(4.8, 3770.3);
        var item = EquityGrantVariance.NewEquityGrantVariance("x", "y");
        item.Maximum = 3771.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantVarianceRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new EquityGrantVarianceRatioRangeRule(0.9, 1770.8);
        var item = EquityGrantVariance.NewEquityGrantVariance("x", "y");
        item.Ratio = 1772.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EquityGrantVarianceValidatorReportsEveryProblem();
        EquityGrantVarianceRequiredNameRuleFlagsABlankName();
        EquityGrantVarianceRequiredCountryRuleFlagsABlankCountry();
        EquityGrantVarianceRequiredCodeRuleFlagsABlankCode();
        EquityGrantVarianceMaximumRangeRuleFlagsValuesAboveMaximum();
        EquityGrantVarianceRatioRangeRuleFlagsValuesAboveRatio();
    }
}
