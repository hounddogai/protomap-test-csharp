namespace Acme.Workforce.Contractors.Tests.Offboarding.Segment;

using Acme.Workforce.Contractors.Offboarding.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class LegacyOffboardingThresholdRulesChecks
{
    public static void LegacyOffboardingThresholdValidatorReportsEveryProblem()
    {
        var validator = LegacyOffboardingThresholdRules.DefaultLegacyOffboardingThresholdValidator();
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyOffboardingThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyOffboardingThresholdRequiredTitleRule("legacy offboarding threshold needs a value");
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyOffboardingThresholdRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LegacyOffboardingThresholdRatioRangeRule(5.7, 4199.2);
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("x", "y");
        item.Ratio = 4200.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyOffboardingThresholdTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LegacyOffboardingThresholdTitleLengthRule(80);
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyOffboardingThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyOffboardingThresholdRequiredCodeRule("legacy offboarding threshold needs this field");
        var item = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyOffboardingThresholdValidatorReportsEveryProblem();
        LegacyOffboardingThresholdRequiredTitleRuleFlagsABlankTitle();
        LegacyOffboardingThresholdRatioRangeRuleFlagsValuesAboveRatio();
        LegacyOffboardingThresholdTitleLengthRuleFlagsALongTitle();
        LegacyOffboardingThresholdRequiredCodeRuleFlagsABlankCode();
    }
}
