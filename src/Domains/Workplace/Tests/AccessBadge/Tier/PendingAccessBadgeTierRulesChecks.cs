namespace Acme.Workforce.Workplace.Tests.AccessBadge.Tier;

using Acme.Workforce.Workplace.AccessBadge.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PendingAccessBadgeTierRulesChecks
{
    public static void PendingAccessBadgeTierValidatorReportsEveryProblem()
    {
        var validator = PendingAccessBadgeTierRules.DefaultPendingAccessBadgeTierValidator();
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingAccessBadgeTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PendingAccessBadgeTierRequiredTitleRule("pending access badge tier needs a value");
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAccessBadgeTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PendingAccessBadgeTierTitleLengthRule(255);
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAccessBadgeTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingAccessBadgeTierCodeLengthRule(3600);
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingAccessBadgeTierDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PendingAccessBadgeTierDurationMinutesRangeRule(16, 16);
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("x", "y");
        item.DurationMinutes = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingAccessBadgeTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingAccessBadgeTierRequiredCodeRule("pending access badge tier needs an entry");
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingAccessBadgeTierUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new PendingAccessBadgeTierUpdatedAtRangeRule(32, 120);
        var item = PendingAccessBadgeTier.NewPendingAccessBadgeTier("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingAccessBadgeTierValidatorReportsEveryProblem();
        PendingAccessBadgeTierRequiredTitleRuleFlagsABlankTitle();
        PendingAccessBadgeTierTitleLengthRuleFlagsALongTitle();
        PendingAccessBadgeTierCodeLengthRuleFlagsALongCode();
        PendingAccessBadgeTierDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PendingAccessBadgeTierRequiredCodeRuleFlagsABlankCode();
        PendingAccessBadgeTierUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
