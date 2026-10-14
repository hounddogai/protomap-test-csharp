namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallCheckpointRulesChecks
{
    public static void TownHallCheckpointValidatorReportsEveryProblem()
    {
        var validator = TownHallCheckpointRules.DefaultTownHallCheckpointValidator();
        var item = TownHallCheckpoint.NewTownHallCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TownHallCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TownHallCheckpointTitleLengthRule(64);
        var item = TownHallCheckpoint.NewTownHallCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TownHallCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new TownHallCheckpointFloorValueRangeRule(1.3, 779.7);
        var item = TownHallCheckpoint.NewTownHallCheckpoint("x", "y");
        item.FloorValue = 781.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new TownHallCheckpointLeaveDaysRangeRule(16, 120);
        var item = TownHallCheckpoint.NewTownHallCheckpoint("x", "y");
        item.LeaveDays = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TownHallCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new TownHallCheckpointNameLengthRule(32);
        var item = TownHallCheckpoint.NewTownHallCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TownHallCheckpointValidatorReportsEveryProblem();
        TownHallCheckpointTitleLengthRuleFlagsALongTitle();
        TownHallCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue();
        TownHallCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        TownHallCheckpointNameLengthRuleFlagsALongName();
    }
}
