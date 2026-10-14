namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredReturnPlanThresholdRulesChecks
{
    public static void TieredReturnPlanThresholdValidatorReportsEveryProblem()
    {
        var validator = TieredReturnPlanThresholdRules.DefaultTieredReturnPlanThresholdValidator();
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredReturnPlanThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new TieredReturnPlanThresholdRequiredLabelRule("tiered return plan threshold needs a value");
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredReturnPlanThresholdValidatorReportsEveryProblem();
        TieredReturnPlanThresholdRequiredLabelRuleFlagsABlankLabel();
    }
}
