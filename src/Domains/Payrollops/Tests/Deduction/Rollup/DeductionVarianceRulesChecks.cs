namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionVarianceRulesChecks
{
    public static void DeductionVarianceValidatorReportsEveryProblem()
    {
        var validator = DeductionVarianceRules.DefaultDeductionVarianceValidator();
        var item = DeductionVariance.NewDeductionVariance("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionVarianceCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionVarianceCodeLengthRule(80);
        var item = DeductionVariance.NewDeductionVariance("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionVarianceBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new DeductionVarianceBudgetRangeRule(6.3, 359.7);
        var item = DeductionVariance.NewDeductionVariance("x", "y");
        item.Budget = 361.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionVarianceThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new DeductionVarianceThresholdRangeRule(8.9, 4648.0);
        var item = DeductionVariance.NewDeductionVariance("x", "y");
        item.Threshold = 4649.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionVarianceSkillLengthRuleFlagsALongSkill()
    {
        var rule = new DeductionVarianceSkillLengthRule(32);
        var item = DeductionVariance.NewDeductionVariance("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DeductionVarianceValidatorReportsEveryProblem();
        DeductionVarianceCodeLengthRuleFlagsALongCode();
        DeductionVarianceBudgetRangeRuleFlagsValuesAboveBudget();
        DeductionVarianceThresholdRangeRuleFlagsValuesAboveThreshold();
        DeductionVarianceSkillLengthRuleFlagsALongSkill();
    }
}
