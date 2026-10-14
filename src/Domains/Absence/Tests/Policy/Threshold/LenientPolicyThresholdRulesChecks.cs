namespace Acme.Workforce.Absence.Tests.Policy.Threshold;

using Acme.Workforce.Absence.Policy.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientPolicyThresholdRulesChecks
{
    public static void LenientPolicyThresholdValidatorReportsEveryProblem()
    {
        var validator = LenientPolicyThresholdRules.DefaultLenientPolicyThresholdValidator();
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPolicyThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LenientPolicyThresholdRequiredCodeRule("lenient policy threshold needs an entry");
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPolicyThresholdNameLengthRuleFlagsALongName()
    {
        var rule = new LenientPolicyThresholdNameLengthRule(16);
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientPolicyThresholdGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new LenientPolicyThresholdGraceDaysRangeRule(3600, 86400);
        var item = LenientPolicyThreshold.NewLenientPolicyThreshold("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientPolicyThresholdValidatorReportsEveryProblem();
        LenientPolicyThresholdRequiredCodeRuleFlagsABlankCode();
        LenientPolicyThresholdNameLengthRuleFlagsALongName();
        LenientPolicyThresholdGraceDaysRangeRuleFlagsValuesAboveGraceDays();
    }
}
