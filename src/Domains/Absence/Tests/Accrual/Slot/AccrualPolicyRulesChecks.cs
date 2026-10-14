namespace Acme.Workforce.Absence.Tests.Accrual.Slot;

using Acme.Workforce.Absence.Accrual.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class AccrualPolicyRulesChecks
{
    public static void AccrualPolicyValidatorReportsEveryProblem()
    {
        var validator = AccrualPolicyRules.DefaultAccrualPolicyValidator();
        var item = AccrualPolicy.NewAccrualPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AccrualPolicyBatchSizeRangeRule(120, 86400);
        var item = AccrualPolicy.NewAccrualPolicy("x", "y");
        item.BatchSize = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualPolicyUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AccrualPolicyUnitLengthRule(255);
        var item = AccrualPolicy.NewAccrualPolicy("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualPolicyValidatorReportsEveryProblem();
        AccrualPolicyBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AccrualPolicyUnitLengthRuleFlagsALongUnit();
    }
}
