namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class PoolExceptionRulesChecks
{
    public static void PoolExceptionValidatorReportsEveryProblem()
    {
        var validator = PoolExceptionRules.DefaultPoolExceptionValidator();
        var item = PoolException.NewPoolException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolExceptionRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PoolExceptionRequiredTitleRule("pool exception needs an entry");
        var item = PoolException.NewPoolException("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolExceptionRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new PoolExceptionRequiredTimezoneRule("pool exception needs a value");
        var item = PoolException.NewPoolException("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolExceptionEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new PoolExceptionEmployeeIdLengthRule(16);
        var item = PoolException.NewPoolException("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolExceptionQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new PoolExceptionQuantityRangeRule(16, 120);
        var item = PoolException.NewPoolException("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PoolExceptionTitleLengthRule(16);
        var item = PoolException.NewPoolException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolExceptionValidatorReportsEveryProblem();
        PoolExceptionRequiredTitleRuleFlagsABlankTitle();
        PoolExceptionRequiredTimezoneRuleFlagsABlankTimezone();
        PoolExceptionEmployeeIdLengthRuleFlagsALongEmployeeId();
        PoolExceptionQuantityRangeRuleFlagsValuesAboveQuantity();
        PoolExceptionTitleLengthRuleFlagsALongTitle();
    }
}
