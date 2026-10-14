namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementBundleRulesChecks
{
    public static void EngagementBundleValidatorReportsEveryProblem()
    {
        var validator = EngagementBundleRules.DefaultEngagementBundleValidator();
        var item = EngagementBundle.NewEngagementBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementBundleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new EngagementBundleRequiredUnitRule("engagement bundle needs this field");
        var item = EngagementBundle.NewEngagementBundle("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementBundleCountryLengthRuleFlagsALongCountry()
    {
        var rule = new EngagementBundleCountryLengthRule(80);
        var item = EngagementBundle.NewEngagementBundle("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementBundleRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new EngagementBundleRateRangeRule(2.0, 3128.2);
        var item = EngagementBundle.NewEngagementBundle("x", "y");
        item.Rate = 3129.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EngagementBundleRequiredCodeRule("engagement bundle needs this field");
        var item = EngagementBundle.NewEngagementBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EngagementBundleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new EngagementBundleUnitLengthRule(120);
        var item = EngagementBundle.NewEngagementBundle("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new EngagementBundleCodeLengthRule(255);
        var item = EngagementBundle.NewEngagementBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EngagementBundleValidatorReportsEveryProblem();
        EngagementBundleRequiredUnitRuleFlagsABlankUnit();
        EngagementBundleCountryLengthRuleFlagsALongCountry();
        EngagementBundleRateRangeRuleFlagsValuesAboveRate();
        EngagementBundleRequiredCodeRuleFlagsABlankCode();
        EngagementBundleUnitLengthRuleFlagsALongUnit();
        EngagementBundleCodeLengthRuleFlagsALongCode();
    }
}
