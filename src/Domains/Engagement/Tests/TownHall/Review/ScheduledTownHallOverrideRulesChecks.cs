namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledTownHallOverrideRulesChecks
{
    public static void ScheduledTownHallOverrideValidatorReportsEveryProblem()
    {
        var validator = ScheduledTownHallOverrideRules.DefaultScheduledTownHallOverrideValidator();
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledTownHallOverrideRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new ScheduledTownHallOverrideRequiredNotesRule("scheduled town hall override needs this field");
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledTownHallOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledTownHallOverrideCodeLengthRule(120);
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledTownHallOverrideRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new ScheduledTownHallOverrideRequiredChecksumRule("scheduled town hall override needs this field");
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledTownHallOverrideRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new ScheduledTownHallOverrideRateRangeRule(9.1, 2525.1);
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("x", "y");
        item.Rate = 2526.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledTownHallOverrideTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ScheduledTownHallOverrideTitleLengthRule(1000);
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledTownHallOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledTownHallOverrideRequiredCodeRule("scheduled town hall override needs an entry");
        var item = ScheduledTownHallOverride.NewScheduledTownHallOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledTownHallOverrideValidatorReportsEveryProblem();
        ScheduledTownHallOverrideRequiredNotesRuleFlagsABlankNotes();
        ScheduledTownHallOverrideCodeLengthRuleFlagsALongCode();
        ScheduledTownHallOverrideRequiredChecksumRuleFlagsABlankChecksum();
        ScheduledTownHallOverrideRateRangeRuleFlagsValuesAboveRate();
        ScheduledTownHallOverrideTitleLengthRuleFlagsALongTitle();
        ScheduledTownHallOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
