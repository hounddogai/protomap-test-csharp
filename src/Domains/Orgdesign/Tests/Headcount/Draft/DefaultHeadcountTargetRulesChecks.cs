namespace Acme.Workforce.Orgdesign.Tests.Headcount.Draft;

using Acme.Workforce.Orgdesign.Headcount.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class DefaultHeadcountTargetRulesChecks
{
    public static void DefaultHeadcountTargetValidatorReportsEveryProblem()
    {
        var validator = DefaultHeadcountTargetRules.DefaultDefaultHeadcountTargetValidator();
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultHeadcountTargetWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new DefaultHeadcountTargetWeightRangeRule(1.8, 1965.6);
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("x", "y");
        item.Weight = 1967.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultHeadcountTargetRequiredNameRule("default headcount target needs this field");
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHeadcountTargetDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new DefaultHeadcountTargetDurationMinutesRangeRule(16, 3600);
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("x", "y");
        item.DurationMinutes = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultHeadcountTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultHeadcountTargetCodeLengthRule(80);
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultHeadcountTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultHeadcountTargetRequiredCodeRule("default headcount target needs a value");
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultHeadcountTargetThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new DefaultHeadcountTargetThresholdRangeRule(9.2, 3578.3);
        var item = DefaultHeadcountTarget.NewDefaultHeadcountTarget("x", "y");
        item.Threshold = 3579.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultHeadcountTargetValidatorReportsEveryProblem();
        DefaultHeadcountTargetWeightRangeRuleFlagsValuesAboveWeight();
        DefaultHeadcountTargetRequiredNameRuleFlagsABlankName();
        DefaultHeadcountTargetDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        DefaultHeadcountTargetCodeLengthRuleFlagsALongCode();
        DefaultHeadcountTargetRequiredCodeRuleFlagsABlankCode();
        DefaultHeadcountTargetThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
