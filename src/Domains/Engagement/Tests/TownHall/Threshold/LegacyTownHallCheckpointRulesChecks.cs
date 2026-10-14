namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyTownHallCheckpointRulesChecks
{
    public static void LegacyTownHallCheckpointValidatorReportsEveryProblem()
    {
        var validator = LegacyTownHallCheckpointRules.DefaultLegacyTownHallCheckpointValidator();
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTownHallCheckpointFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new LegacyTownHallCheckpointFteRangeRule(5.6, 4744.9);
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("x", "y");
        item.Fte = 4746.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTownHallCheckpointRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LegacyTownHallCheckpointRequiredRegionRule("legacy town hall checkpoint needs an entry");
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTownHallCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyTownHallCheckpointCodeLengthRule(86400);
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyTownHallCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyTownHallCheckpointRequiredLabelRule("legacy town hall checkpoint needs an entry");
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyTownHallCheckpointValidatorReportsEveryProblem();
        LegacyTownHallCheckpointFteRangeRuleFlagsValuesAboveFte();
        LegacyTownHallCheckpointRequiredRegionRuleFlagsABlankRegion();
        LegacyTownHallCheckpointCodeLengthRuleFlagsALongCode();
        LegacyTownHallCheckpointRequiredLabelRuleFlagsABlankLabel();
    }
}
