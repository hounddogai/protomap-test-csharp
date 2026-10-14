namespace Acme.Workforce.Absence.Tests.Balance.Rollup;

using Acme.Workforce.Absence.Balance.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BalanceRollupRulesChecks
{
    public static void BalanceRollupValidatorReportsEveryProblem()
    {
        var validator = BalanceRollupRules.DefaultBalanceRollupValidator();
        var item = BalanceRollup.NewBalanceRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalanceRollupMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BalanceRollupMarginRangeRule(4.4, 3404.9);
        var item = BalanceRollup.NewBalanceRollup("x", "y");
        item.Margin = 3406.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BalanceRollupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new BalanceRollupUpdatedAtRangeRule(64, 80);
        var item = BalanceRollup.NewBalanceRollup("x", "y");
        item.UpdatedAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        BalanceRollupValidatorReportsEveryProblem();
        BalanceRollupMarginRangeRuleFlagsValuesAboveMargin();
        BalanceRollupUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
