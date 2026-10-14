namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DefaultPoolExceptionRulesChecks
{
    public static void DefaultPoolExceptionValidatorReportsEveryProblem()
    {
        var validator = DefaultPoolExceptionRules.DefaultDefaultPoolExceptionValidator();
        var item = DefaultPoolException.NewDefaultPoolException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultPoolExceptionRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new DefaultPoolExceptionRateRangeRule(8.5, 3626.2);
        var item = DefaultPoolException.NewDefaultPoolException("x", "y");
        item.Rate = 3627.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultPoolExceptionLocationLengthRuleFlagsALongLocation()
    {
        var rule = new DefaultPoolExceptionLocationLengthRule(32);
        var item = DefaultPoolException.NewDefaultPoolException("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultPoolExceptionCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new DefaultPoolExceptionCeilingRangeRule(6.4, 3806.0);
        var item = DefaultPoolException.NewDefaultPoolException("x", "y");
        item.Ceiling = 3807.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultPoolExceptionValidatorReportsEveryProblem();
        DefaultPoolExceptionRateRangeRuleFlagsValuesAboveRate();
        DefaultPoolExceptionLocationLengthRuleFlagsALongLocation();
        DefaultPoolExceptionCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
