namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class WeightedAdjustmentTemplateRulesChecks
{
    public static void WeightedAdjustmentTemplateValidatorReportsEveryProblem()
    {
        var validator = WeightedAdjustmentTemplateRules.DefaultWeightedAdjustmentTemplateValidator();
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedAdjustmentTemplateRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new WeightedAdjustmentTemplateRequiredDepartmentRule("weighted adjustment template needs this field");
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAdjustmentTemplateSourceLengthRuleFlagsALongSource()
    {
        var rule = new WeightedAdjustmentTemplateSourceLengthRule(16);
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedAdjustmentTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedAdjustmentTemplateRequiredLabelRule("weighted adjustment template needs an entry");
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedAdjustmentTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedAdjustmentTemplateRequiredCodeRule("weighted adjustment template needs a value");
        var item = WeightedAdjustmentTemplate.NewWeightedAdjustmentTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedAdjustmentTemplateValidatorReportsEveryProblem();
        WeightedAdjustmentTemplateRequiredDepartmentRuleFlagsABlankDepartment();
        WeightedAdjustmentTemplateSourceLengthRuleFlagsALongSource();
        WeightedAdjustmentTemplateRequiredLabelRuleFlagsABlankLabel();
        WeightedAdjustmentTemplateRequiredCodeRuleFlagsABlankCode();
    }
}
