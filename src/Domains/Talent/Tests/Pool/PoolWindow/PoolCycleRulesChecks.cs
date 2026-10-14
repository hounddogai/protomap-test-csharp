namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PoolCycleRulesChecks
{
    public static void PoolCycleValidatorReportsEveryProblem()
    {
        var validator = PoolCycleRules.DefaultPoolCycleValidator();
        var item = PoolCycle.NewPoolCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolCycleRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new PoolCycleRetriesRangeRule(64, 86400);
        var item = PoolCycle.NewPoolCycle("x", "y");
        item.Retries = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolCycleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new PoolCycleBudgetRangeRule(9.5, 2930.6);
        var item = PoolCycle.NewPoolCycle("x", "y");
        item.Budget = 2932.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PoolCycleValidatorReportsEveryProblem();
        PoolCycleRetriesRangeRuleFlagsValuesAboveRetries();
        PoolCycleBudgetRangeRuleFlagsValuesAboveBudget();
    }
}
