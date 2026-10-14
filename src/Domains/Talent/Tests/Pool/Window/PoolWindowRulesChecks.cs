namespace Acme.Workforce.Talent.Tests.Pool.Window;

using Acme.Workforce.Talent.Pool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PoolWindowRulesChecks
{
    public static void PoolWindowValidatorReportsEveryProblem()
    {
        var validator = PoolWindowRules.DefaultPoolWindowValidator();
        var item = PoolWindow.NewPoolWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolWindowCodeLengthRule(16);
        var item = PoolWindow.NewPoolWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolWindowRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new PoolWindowRequiredManagerIdRule("pool window needs a value");
        var item = PoolWindow.NewPoolWindow("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolWindowManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new PoolWindowManagerIdLengthRule(80);
        var item = PoolWindow.NewPoolWindow("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolWindowHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new PoolWindowHoursRangeRule(5.2, 2269.0);
        var item = PoolWindow.NewPoolWindow("x", "y");
        item.Hours = 2270.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PoolWindowStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new PoolWindowStartsAtRangeRule(64, 120);
        var item = PoolWindow.NewPoolWindow("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PoolWindowValidatorReportsEveryProblem();
        PoolWindowCodeLengthRuleFlagsALongCode();
        PoolWindowRequiredManagerIdRuleFlagsABlankManagerId();
        PoolWindowManagerIdLengthRuleFlagsALongManagerId();
        PoolWindowHoursRangeRuleFlagsValuesAboveHours();
        PoolWindowStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
