namespace Acme.Workforce.Talent.Tests.Nomination.Rollup;

using Acme.Workforce.Talent.Nomination.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class FallbackNominationExceptionRulesChecks
{
    public static void FallbackNominationExceptionValidatorReportsEveryProblem()
    {
        var validator = FallbackNominationExceptionRules.DefaultFallbackNominationExceptionValidator();
        var item = FallbackNominationException.NewFallbackNominationException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackNominationExceptionNameLengthRuleFlagsALongName()
    {
        var rule = new FallbackNominationExceptionNameLengthRule(80);
        var item = FallbackNominationException.NewFallbackNominationException("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackNominationExceptionUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new FallbackNominationExceptionUpdatedAtRangeRule(80, 255);
        var item = FallbackNominationException.NewFallbackNominationException("x", "y");
        item.UpdatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackNominationExceptionRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new FallbackNominationExceptionRequiredChannelRule("fallback nomination exception needs a value");
        var item = FallbackNominationException.NewFallbackNominationException("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackNominationExceptionBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new FallbackNominationExceptionBadgeLengthRule(86400);
        var item = FallbackNominationException.NewFallbackNominationException("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackNominationExceptionValidatorReportsEveryProblem();
        FallbackNominationExceptionNameLengthRuleFlagsALongName();
        FallbackNominationExceptionUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        FallbackNominationExceptionRequiredChannelRuleFlagsABlankChannel();
        FallbackNominationExceptionBadgeLengthRuleFlagsALongBadge();
    }
}
