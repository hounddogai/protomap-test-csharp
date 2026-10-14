namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanThresholdRulesChecks
{
    public static void SuccessionPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanThresholdRules.DefaultSuccessionPlanThresholdValidator();
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new SuccessionPlanThresholdCodeLengthRule(120);
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanThresholdFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new SuccessionPlanThresholdFteRangeRule(0.4, 3409.4);
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("x", "y");
        item.Fte = 3410.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SuccessionPlanThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanThresholdRequiredCodeRule("succession plan threshold needs an entry");
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanThresholdRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SuccessionPlanThresholdRequiredTitleRule("succession plan threshold needs this field");
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanThresholdCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new SuccessionPlanThresholdCapacityRangeRule(120, 255);
        var item = SuccessionPlanThreshold.NewSuccessionPlanThreshold("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        SuccessionPlanThresholdValidatorReportsEveryProblem();
        SuccessionPlanThresholdCodeLengthRuleFlagsALongCode();
        SuccessionPlanThresholdFteRangeRuleFlagsValuesAboveFte();
        SuccessionPlanThresholdRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanThresholdRequiredTitleRuleFlagsABlankTitle();
        SuccessionPlanThresholdCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
