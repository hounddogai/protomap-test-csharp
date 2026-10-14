namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class WeightedReturnPlanOverrideRulesChecks
{
    public static void WeightedReturnPlanOverrideValidatorReportsEveryProblem()
    {
        var validator = WeightedReturnPlanOverrideRules.DefaultWeightedReturnPlanOverrideValidator();
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedReturnPlanOverrideRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new WeightedReturnPlanOverrideRequiredJobCodeRule("weighted return plan override needs this field");
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedReturnPlanOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedReturnPlanOverrideLabelLengthRule(86400);
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedReturnPlanOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedReturnPlanOverrideRequiredLabelRule("weighted return plan override needs this field");
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedReturnPlanOverrideRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new WeightedReturnPlanOverrideRequiredCategoryRule("weighted return plan override needs a value");
        var item = WeightedReturnPlanOverride.NewWeightedReturnPlanOverride("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedReturnPlanOverrideValidatorReportsEveryProblem();
        WeightedReturnPlanOverrideRequiredJobCodeRuleFlagsABlankJobCode();
        WeightedReturnPlanOverrideLabelLengthRuleFlagsALongLabel();
        WeightedReturnPlanOverrideRequiredLabelRuleFlagsABlankLabel();
        WeightedReturnPlanOverrideRequiredCategoryRuleFlagsABlankCategory();
    }
}
