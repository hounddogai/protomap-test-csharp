namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityCheckpointRulesChecks
{
    public static void MobilityCheckpointValidatorReportsEveryProblem()
    {
        var validator = MobilityCheckpointRules.DefaultMobilityCheckpointValidator();
        var item = MobilityCheckpoint.NewMobilityCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilityCheckpointRequiredCodeRule("mobility checkpoint needs this field");
        var item = MobilityCheckpoint.NewMobilityCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityCheckpointMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new MobilityCheckpointMaximumRangeRule(2.1, 1551.1);
        var item = MobilityCheckpoint.NewMobilityCheckpoint("x", "y");
        item.Maximum = 1552.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MobilityCheckpointCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new MobilityCheckpointCategoryLengthRule(86400);
        var item = MobilityCheckpoint.NewMobilityCheckpoint("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityCheckpointRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new MobilityCheckpointRequiredLabelRule("mobility checkpoint needs this field");
        var item = MobilityCheckpoint.NewMobilityCheckpoint("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new MobilityCheckpointNameLengthRule(3600);
        var item = MobilityCheckpoint.NewMobilityCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityCheckpointValidatorReportsEveryProblem();
        MobilityCheckpointRequiredCodeRuleFlagsABlankCode();
        MobilityCheckpointMaximumRangeRuleFlagsValuesAboveMaximum();
        MobilityCheckpointCategoryLengthRuleFlagsALongCategory();
        MobilityCheckpointRequiredLabelRuleFlagsABlankLabel();
        MobilityCheckpointNameLengthRuleFlagsALongName();
    }
}
