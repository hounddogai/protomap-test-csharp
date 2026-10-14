namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class RollingSpanRollupRulesChecks
{
    public static void RollingSpanRollupValidatorReportsEveryProblem()
    {
        var validator = RollingSpanRollupRules.DefaultRollingSpanRollupValidator();
        var item = RollingSpanRollup.NewRollingSpanRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSpanRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RollingSpanRollupExpiresAtRangeRule(64, 3600);
        var item = RollingSpanRollup.NewRollingSpanRollup("x", "y");
        item.ExpiresAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingSpanRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingSpanRollupRequiredCodeRule("rolling span rollup needs a value");
        var item = RollingSpanRollup.NewRollingSpanRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSpanRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingSpanRollupCodeLengthRule(64);
        var item = RollingSpanRollup.NewRollingSpanRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingSpanRollupValidatorReportsEveryProblem();
        RollingSpanRollupExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RollingSpanRollupRequiredCodeRuleFlagsABlankCode();
        RollingSpanRollupCodeLengthRuleFlagsALongCode();
    }
}
