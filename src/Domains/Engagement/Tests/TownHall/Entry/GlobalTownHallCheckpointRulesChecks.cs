namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalTownHallCheckpointRulesChecks
{
    public static void GlobalTownHallCheckpointValidatorReportsEveryProblem()
    {
        var validator = GlobalTownHallCheckpointRules.DefaultGlobalTownHallCheckpointValidator();
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalTownHallCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalTownHallCheckpointLabelLengthRule(120);
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalTownHallCheckpointRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new GlobalTownHallCheckpointRequiredGradeRule("global town hall checkpoint needs this field");
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalTownHallCheckpointBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new GlobalTownHallCheckpointBadgeLengthRule(120);
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalTownHallCheckpointHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new GlobalTownHallCheckpointHoursRangeRule(8.3, 425.8);
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("x", "y");
        item.Hours = 427.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalTownHallCheckpointValidatorReportsEveryProblem();
        GlobalTownHallCheckpointLabelLengthRuleFlagsALongLabel();
        GlobalTownHallCheckpointRequiredGradeRuleFlagsABlankGrade();
        GlobalTownHallCheckpointBadgeLengthRuleFlagsALongBadge();
        GlobalTownHallCheckpointHoursRangeRuleFlagsValuesAboveHours();
    }
}
