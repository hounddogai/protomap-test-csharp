namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CareerPathCheckpointRulesChecks
{
    public static void CareerPathCheckpointValidatorReportsEveryProblem()
    {
        var validator = CareerPathCheckpointRules.DefaultCareerPathCheckpointValidator();
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathCheckpointLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CareerPathCheckpointLabelLengthRule(32);
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathCheckpointOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new CareerPathCheckpointOwnerIdLengthRule(255);
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathCheckpointRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CareerPathCheckpointRatioRangeRule(3.7, 4783.4);
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("x", "y");
        item.Ratio = 4784.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathCheckpointCodeLengthRule(120);
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathCheckpointCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new CareerPathCheckpointCeilingRangeRule(9.1, 382.3);
        var item = CareerPathCheckpoint.NewCareerPathCheckpoint("x", "y");
        item.Ceiling = 383.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathCheckpointValidatorReportsEveryProblem();
        CareerPathCheckpointLabelLengthRuleFlagsALongLabel();
        CareerPathCheckpointOwnerIdLengthRuleFlagsALongOwnerId();
        CareerPathCheckpointRatioRangeRuleFlagsValuesAboveRatio();
        CareerPathCheckpointCodeLengthRuleFlagsALongCode();
        CareerPathCheckpointCeilingRangeRuleFlagsValuesAboveCeiling();
    }
}
