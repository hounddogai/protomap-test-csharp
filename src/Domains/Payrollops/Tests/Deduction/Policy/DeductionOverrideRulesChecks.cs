namespace Acme.Workforce.Payrollops.Tests.Deduction.Policy;

using Acme.Workforce.Payrollops.Deduction.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DeductionOverrideRulesChecks
{
    public static void DeductionOverrideValidatorReportsEveryProblem()
    {
        var validator = DeductionOverrideRules.DefaultDeductionOverrideValidator();
        var item = DeductionOverride.NewDeductionOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionOverrideUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new DeductionOverrideUpdatedAtRangeRule(32, 32);
        var item = DeductionOverride.NewDeductionOverride("x", "y");
        item.UpdatedAt = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionOverrideCodeLengthRule(80);
        var item = DeductionOverride.NewDeductionOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionOverrideRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new DeductionOverrideRatioRangeRule(3.8, 933.1);
        var item = DeductionOverride.NewDeductionOverride("x", "y");
        item.Ratio = 934.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionOverrideValidatorReportsEveryProblem();
        DeductionOverrideUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        DeductionOverrideCodeLengthRuleFlagsALongCode();
        DeductionOverrideRatioRangeRuleFlagsValuesAboveRatio();
    }
}
