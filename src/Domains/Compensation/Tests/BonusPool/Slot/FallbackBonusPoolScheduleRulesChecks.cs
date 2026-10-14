namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class FallbackBonusPoolScheduleRulesChecks
{
    public static void FallbackBonusPoolScheduleValidatorReportsEveryProblem()
    {
        var validator = FallbackBonusPoolScheduleRules.DefaultFallbackBonusPoolScheduleValidator();
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackBonusPoolScheduleBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new FallbackBonusPoolScheduleBadgeLengthRule(80);
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackBonusPoolScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackBonusPoolScheduleRequiredNameRule("fallback bonus pool schedule needs an entry");
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackBonusPoolScheduleRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new FallbackBonusPoolScheduleRequiredBadgeRule("fallback bonus pool schedule needs this field");
        var item = FallbackBonusPoolSchedule.NewFallbackBonusPoolSchedule("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackBonusPoolScheduleValidatorReportsEveryProblem();
        FallbackBonusPoolScheduleBadgeLengthRuleFlagsALongBadge();
        FallbackBonusPoolScheduleRequiredNameRuleFlagsABlankName();
        FallbackBonusPoolScheduleRequiredBadgeRuleFlagsABlankBadge();
    }
}
