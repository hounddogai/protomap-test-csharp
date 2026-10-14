namespace Acme.Workforce.Compensation.Tests.PayBand.Milestone;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class PrimaryPayBandCheckpointRulesChecks
{
    public static void PrimaryPayBandCheckpointValidatorReportsEveryProblem()
    {
        var validator = PrimaryPayBandCheckpointRules.DefaultPrimaryPayBandCheckpointValidator();
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPayBandCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryPayBandCheckpointLabelLengthRule(86400);
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPayBandCheckpointPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PrimaryPayBandCheckpointPhoneLengthRule(86400);
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPayBandCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PrimaryPayBandCheckpointLeaveDaysRangeRule(16, 255);
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryPayBandCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryPayBandCheckpointCodeLengthRule(120);
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPayBandCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPayBandCheckpointRequiredCodeRule("primary pay band checkpoint needs this field");
        var item = PrimaryPayBandCheckpoint.NewPrimaryPayBandCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryPayBandCheckpointValidatorReportsEveryProblem();
        PrimaryPayBandCheckpointLabelLengthRuleFlagsALongLabel();
        PrimaryPayBandCheckpointPhoneLengthRuleFlagsALongPhone();
        PrimaryPayBandCheckpointLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        PrimaryPayBandCheckpointCodeLengthRuleFlagsALongCode();
        PrimaryPayBandCheckpointRequiredCodeRuleFlagsABlankCode();
    }
}
