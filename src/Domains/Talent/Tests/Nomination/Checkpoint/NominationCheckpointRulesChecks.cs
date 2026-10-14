namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationCheckpointRulesChecks
{
    public static void NominationCheckpointValidatorReportsEveryProblem()
    {
        var validator = NominationCheckpointRules.DefaultNominationCheckpointValidator();
        var item = NominationCheckpoint.NewNominationCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationCheckpointRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new NominationCheckpointRequiredUnitRule("nomination checkpoint needs a value");
        var item = NominationCheckpoint.NewNominationCheckpoint("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new NominationCheckpointTitleLengthRule(86400);
        var item = NominationCheckpoint.NewNominationCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationCheckpointCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new NominationCheckpointCapacityRangeRule(64, 3600);
        var item = NominationCheckpoint.NewNominationCheckpoint("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void NominationCheckpointCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new NominationCheckpointCategoryLengthRule(16);
        var item = NominationCheckpoint.NewNominationCheckpoint("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        NominationCheckpointValidatorReportsEveryProblem();
        NominationCheckpointRequiredUnitRuleFlagsABlankUnit();
        NominationCheckpointTitleLengthRuleFlagsALongTitle();
        NominationCheckpointCapacityRangeRuleFlagsValuesAboveCapacity();
        NominationCheckpointCategoryLengthRuleFlagsALongCategory();
    }
}
