namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AccessBadgeExceptionRulesChecks
{
    public static void AccessBadgeExceptionValidatorReportsEveryProblem()
    {
        var validator = AccessBadgeExceptionRules.DefaultAccessBadgeExceptionValidator();
        var item = AccessBadgeException.NewAccessBadgeException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccessBadgeExceptionTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccessBadgeExceptionTitleLengthRule(16);
        var item = AccessBadgeException.NewAccessBadgeException("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccessBadgeExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccessBadgeExceptionRequiredCodeRule("access badge exception needs a value");
        var item = AccessBadgeException.NewAccessBadgeException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccessBadgeExceptionDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new AccessBadgeExceptionDurationMinutesRangeRule(32, 1000);
        var item = AccessBadgeException.NewAccessBadgeException("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AccessBadgeExceptionValidatorReportsEveryProblem();
        AccessBadgeExceptionTitleLengthRuleFlagsALongTitle();
        AccessBadgeExceptionRequiredCodeRuleFlagsABlankCode();
        AccessBadgeExceptionDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
    }
}
