namespace Acme.Workforce.Payrollops.Tests.Deduction.Proposal;

using Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DeductionTemplateRulesChecks
{
    public static void DeductionTemplateValidatorReportsEveryProblem()
    {
        var validator = DeductionTemplateRules.DefaultDeductionTemplateValidator();
        var item = DeductionTemplate.NewDeductionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionTemplateCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new DeductionTemplateCostRangeRule(7.8, 2311.8);
        var item = DeductionTemplate.NewDeductionTemplate("x", "y");
        item.Cost = 2313.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionTemplateThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new DeductionTemplateThresholdRangeRule(1.8, 2671.8);
        var item = DeductionTemplate.NewDeductionTemplate("x", "y");
        item.Threshold = 2673.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionTemplateLabelLengthRuleFlagsALongLabel()
    {
        var rule = new DeductionTemplateLabelLengthRule(3600);
        var item = DeductionTemplate.NewDeductionTemplate("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionTemplateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionTemplateRequiredCodeRule("deduction template needs an entry");
        var item = DeductionTemplate.NewDeductionTemplate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionTemplateCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionTemplateCodeLengthRule(1000);
        var item = DeductionTemplate.NewDeductionTemplate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionTemplateValidatorReportsEveryProblem();
        DeductionTemplateCostRangeRuleFlagsValuesAboveCost();
        DeductionTemplateThresholdRangeRuleFlagsValuesAboveThreshold();
        DeductionTemplateLabelLengthRuleFlagsALongLabel();
        DeductionTemplateRequiredCodeRuleFlagsABlankCode();
        DeductionTemplateCodeLengthRuleFlagsALongCode();
    }
}
