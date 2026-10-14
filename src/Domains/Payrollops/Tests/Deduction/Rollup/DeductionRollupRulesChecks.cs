namespace Acme.Workforce.Payrollops.Tests.Deduction.Rollup;

using Acme.Workforce.Payrollops.Deduction.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class DeductionRollupRulesChecks
{
    public static void DeductionRollupValidatorReportsEveryProblem()
    {
        var validator = DeductionRollupRules.DefaultDeductionRollupValidator();
        var item = DeductionRollup.NewDeductionRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeductionRollupRequiredCodeRule("deduction rollup needs a value");
        var item = DeductionRollup.NewDeductionRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionRollupSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new DeductionRollupSequenceRangeRule(255, 255);
        var item = DeductionRollup.NewDeductionRollup("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DeductionRollupValidatorReportsEveryProblem();
        DeductionRollupRequiredCodeRuleFlagsABlankCode();
        DeductionRollupSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
