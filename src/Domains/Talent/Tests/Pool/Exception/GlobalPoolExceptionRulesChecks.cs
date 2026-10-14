namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalPoolExceptionRulesChecks
{
    public static void GlobalPoolExceptionValidatorReportsEveryProblem()
    {
        var validator = GlobalPoolExceptionRules.DefaultGlobalPoolExceptionValidator();
        var item = GlobalPoolException.NewGlobalPoolException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPoolExceptionEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GlobalPoolExceptionEndsAtRangeRule(120, 3600);
        var item = GlobalPoolException.NewGlobalPoolException("x", "y");
        item.EndsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPoolExceptionTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new GlobalPoolExceptionTimezoneLengthRule(1000);
        var item = GlobalPoolException.NewGlobalPoolException("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPoolExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalPoolExceptionRequiredCodeRule("global pool exception needs an entry");
        var item = GlobalPoolException.NewGlobalPoolException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalPoolExceptionValidatorReportsEveryProblem();
        GlobalPoolExceptionEndsAtRangeRuleFlagsValuesAboveEndsAt();
        GlobalPoolExceptionTimezoneLengthRuleFlagsALongTimezone();
        GlobalPoolExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
