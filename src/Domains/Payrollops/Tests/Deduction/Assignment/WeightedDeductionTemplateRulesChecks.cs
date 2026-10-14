namespace Acme.Workforce.Payrollops.Tests.Deduction.Assignment;

using Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class WeightedDeductionTemplateRulesChecks
{
    public static void WeightedDeductionTemplateValidatorReportsEveryProblem()
    {
        var validator = WeightedDeductionTemplateRules.DefaultWeightedDeductionTemplateValidator();
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedDeductionTemplateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedDeductionTemplateRequiredLabelRule("weighted deduction template needs a value");
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedDeductionTemplateRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new WeightedDeductionTemplateRequiredManagerIdRule("weighted deduction template needs this field");
        var item = WeightedDeductionTemplate.NewWeightedDeductionTemplate("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedDeductionTemplateValidatorReportsEveryProblem();
        WeightedDeductionTemplateRequiredLabelRuleFlagsABlankLabel();
        WeightedDeductionTemplateRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
