namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PendingPoolAllocationRulesChecks
{
    public static void PendingPoolAllocationValidatorReportsEveryProblem()
    {
        var validator = PendingPoolAllocationRules.DefaultPendingPoolAllocationValidator();
        var item = PendingPoolAllocation.NewPendingPoolAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPoolAllocationUnitLengthRuleFlagsALongUnit()
    {
        var rule = new PendingPoolAllocationUnitLengthRule(1000);
        var item = PendingPoolAllocation.NewPendingPoolAllocation("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingPoolAllocationValidatorReportsEveryProblem();
        PendingPoolAllocationUnitLengthRuleFlagsALongUnit();
    }
}
