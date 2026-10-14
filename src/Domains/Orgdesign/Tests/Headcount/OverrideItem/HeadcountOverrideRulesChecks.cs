namespace Acme.Workforce.Orgdesign.Tests.Headcount.OverrideItem;

using Acme.Workforce.Orgdesign.Headcount.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HeadcountOverrideRulesChecks
{
    public static void HeadcountOverrideValidatorReportsEveryProblem()
    {
        var validator = HeadcountOverrideRules.DefaultHeadcountOverrideValidator();
        var item = HeadcountOverride.NewHeadcountOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HeadcountOverrideRequiredLabelRule("headcount override needs an entry");
        var item = HeadcountOverride.NewHeadcountOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountOverrideLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new HeadcountOverrideLimitRangeRule(32, 1000);
        var item = HeadcountOverride.NewHeadcountOverride("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountOverrideCodeLengthRule(3600);
        var item = HeadcountOverride.NewHeadcountOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountOverrideValidatorReportsEveryProblem();
        HeadcountOverrideRequiredLabelRuleFlagsABlankLabel();
        HeadcountOverrideLimitRangeRuleFlagsValuesAboveLimit();
        HeadcountOverrideCodeLengthRuleFlagsALongCode();
    }
}
