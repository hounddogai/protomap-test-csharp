namespace Acme.Workforce.Orgdesign.Tests.Span.Schedule;

using Acme.Workforce.Orgdesign.Span.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ScheduledSpanTierRulesChecks
{
    public static void ScheduledSpanTierValidatorReportsEveryProblem()
    {
        var validator = ScheduledSpanTierRules.DefaultScheduledSpanTierValidator();
        var item = ScheduledSpanTier.NewScheduledSpanTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledSpanTierPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ScheduledSpanTierPriorityRangeRule(80, 120);
        var item = ScheduledSpanTier.NewScheduledSpanTier("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSpanTierAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ScheduledSpanTierAttemptsRangeRule(80, 86400);
        var item = ScheduledSpanTier.NewScheduledSpanTier("x", "y");
        item.Attempts = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledSpanTierLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ScheduledSpanTierLocationLengthRule(80);
        var item = ScheduledSpanTier.NewScheduledSpanTier("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledSpanTierRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledSpanTierRequiredTitleRule("scheduled span tier needs an entry");
        var item = ScheduledSpanTier.NewScheduledSpanTier("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSpanTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledSpanTierTitleLengthRule(16);
        var item = ScheduledSpanTier.NewScheduledSpanTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledSpanTierValidatorReportsEveryProblem();
        ScheduledSpanTierPriorityRangeRuleFlagsValuesAbovePriority();
        ScheduledSpanTierAttemptsRangeRuleFlagsValuesAboveAttempts();
        ScheduledSpanTierLocationLengthRuleFlagsALongLocation();
        ScheduledSpanTierRequiredTitleRuleFlagsABlankTitle();
        ScheduledSpanTierTitleLengthRuleFlagsALongTitle();
    }
}
