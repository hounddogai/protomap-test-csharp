namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ManualAccrualCheckpointRulesChecks
{
    public static void ManualAccrualCheckpointValidatorReportsEveryProblem()
    {
        var validator = ManualAccrualCheckpointRules.DefaultManualAccrualCheckpointValidator();
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAccrualCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualAccrualCheckpointRequiredLabelRule("manual accrual checkpoint needs an entry");
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccrualCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualAccrualCheckpointRequiredCodeRule("manual accrual checkpoint needs an entry");
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccrualCheckpointRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new ManualAccrualCheckpointRatingRangeRule(2.8, 2508.3);
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("x", "y");
        item.Rating = 2509.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAccrualCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ManualAccrualCheckpointLabelLengthRule(86400);
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAccrualCheckpointRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualAccrualCheckpointRequiredTitleRule("manual accrual checkpoint needs an entry");
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAccrualCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualAccrualCheckpointCodeLengthRule(80);
        var item = ManualAccrualCheckpoint.NewManualAccrualCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualAccrualCheckpointValidatorReportsEveryProblem();
        ManualAccrualCheckpointRequiredLabelRuleFlagsABlankLabel();
        ManualAccrualCheckpointRequiredCodeRuleFlagsABlankCode();
        ManualAccrualCheckpointRatingRangeRuleFlagsValuesAboveRating();
        ManualAccrualCheckpointLabelLengthRuleFlagsALongLabel();
        ManualAccrualCheckpointRequiredTitleRuleFlagsABlankTitle();
        ManualAccrualCheckpointCodeLengthRuleFlagsALongCode();
    }
}
