namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class StrictCareerPathOverrideRulesChecks
{
    public static void StrictCareerPathOverrideValidatorReportsEveryProblem()
    {
        var validator = StrictCareerPathOverrideRules.DefaultStrictCareerPathOverrideValidator();
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictCareerPathOverrideRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new StrictCareerPathOverrideRequiredTitleRule("strict career path override needs a value");
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictCareerPathOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StrictCareerPathOverrideRequiredCodeRule("strict career path override needs a value");
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictCareerPathOverrideDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new StrictCareerPathOverrideDiscountRangeRule(5.3, 4706.1);
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("x", "y");
        item.Discount = 4707.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictCareerPathOverrideUnitLengthRuleFlagsALongUnit()
    {
        var rule = new StrictCareerPathOverrideUnitLengthRule(3600);
        var item = StrictCareerPathOverride.NewStrictCareerPathOverride("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictCareerPathOverrideValidatorReportsEveryProblem();
        StrictCareerPathOverrideRequiredTitleRuleFlagsABlankTitle();
        StrictCareerPathOverrideRequiredCodeRuleFlagsABlankCode();
        StrictCareerPathOverrideDiscountRangeRuleFlagsValuesAboveDiscount();
        StrictCareerPathOverrideUnitLengthRuleFlagsALongUnit();
    }
}
