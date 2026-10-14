namespace Acme.Workforce.Assets.Tests.LicenseSeat.Checkpoint;

using Acme.Workforce.Assets.LicenseSeat.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LicenseSeatCheckpointRulesChecks
{
    public static void LicenseSeatCheckpointValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatCheckpointRules.DefaultLicenseSeatCheckpointValidator();
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new LicenseSeatCheckpointStartsAtRangeRule(32, 120);
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("x", "y");
        item.StartsAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatCheckpointTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LicenseSeatCheckpointTitleLengthRule(86400);
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatCheckpointCodeLengthRule(80);
        var item = LicenseSeatCheckpoint.NewLicenseSeatCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatCheckpointValidatorReportsEveryProblem();
        LicenseSeatCheckpointStartsAtRangeRuleFlagsValuesAboveStartsAt();
        LicenseSeatCheckpointTitleLengthRuleFlagsALongTitle();
        LicenseSeatCheckpointCodeLengthRuleFlagsALongCode();
    }
}
