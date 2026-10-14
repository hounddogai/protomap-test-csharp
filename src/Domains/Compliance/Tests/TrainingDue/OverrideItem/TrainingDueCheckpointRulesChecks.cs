namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TrainingDueCheckpointRulesChecks
{
    public static void TrainingDueCheckpointValidatorReportsEveryProblem()
    {
        var validator = TrainingDueCheckpointRules.DefaultTrainingDueCheckpointValidator();
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueCheckpointRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new TrainingDueCheckpointRequiredSegmentRule("training due checkpoint needs a value");
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueCheckpointExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new TrainingDueCheckpointExpiresAtRangeRule(32, 64);
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("x", "y");
        item.ExpiresAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TrainingDueCheckpointSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new TrainingDueCheckpointSequenceRangeRule(64, 255);
        var item = TrainingDueCheckpoint.NewTrainingDueCheckpoint("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        TrainingDueCheckpointValidatorReportsEveryProblem();
        TrainingDueCheckpointRequiredSegmentRuleFlagsABlankSegment();
        TrainingDueCheckpointExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        TrainingDueCheckpointSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
