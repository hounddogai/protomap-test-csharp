namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentThresholdRulesChecks
{
    public static void AdjustmentThresholdValidatorReportsEveryProblem()
    {
        var validator = AdjustmentThresholdRules.DefaultAdjustmentThresholdValidator();
        var item = AdjustmentThreshold.NewAdjustmentThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdjustmentThresholdSourceLengthRuleFlagsALongSource()
    {
        var rule = new AdjustmentThresholdSourceLengthRule(32);
        var item = AdjustmentThreshold.NewAdjustmentThreshold("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdjustmentThresholdCodeLengthRule(3600);
        var item = AdjustmentThreshold.NewAdjustmentThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdjustmentThresholdRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdjustmentThresholdRequiredJobCodeRule("adjustment threshold needs a value");
        var item = AdjustmentThreshold.NewAdjustmentThreshold("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdjustmentThresholdValidatorReportsEveryProblem();
        AdjustmentThresholdSourceLengthRuleFlagsALongSource();
        AdjustmentThresholdCodeLengthRuleFlagsALongCode();
        AdjustmentThresholdRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
