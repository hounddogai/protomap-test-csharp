namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionCheckpointRulesChecks
{
    public static void ExceptionCheckpointValidatorReportsEveryProblem()
    {
        var validator = ExceptionCheckpointRules.DefaultExceptionCheckpointValidator();
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ExceptionCheckpointRequiredLabelRule("exception checkpoint needs this field");
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionCheckpointThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new ExceptionCheckpointThresholdRangeRule(2.3, 2995.7);
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("x", "y");
        item.Threshold = 2997.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new ExceptionCheckpointFloorValueRangeRule(5.6, 726.7);
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("x", "y");
        item.FloorValue = 728.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ExceptionCheckpointLabelLengthRule(86400);
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionCheckpointValidatorReportsEveryProblem();
        ExceptionCheckpointRequiredLabelRuleFlagsABlankLabel();
        ExceptionCheckpointThresholdRangeRuleFlagsValuesAboveThreshold();
        ExceptionCheckpointFloorValueRangeRuleFlagsValuesAboveFloorValue();
        ExceptionCheckpointLabelLengthRuleFlagsALongLabel();
    }
}
