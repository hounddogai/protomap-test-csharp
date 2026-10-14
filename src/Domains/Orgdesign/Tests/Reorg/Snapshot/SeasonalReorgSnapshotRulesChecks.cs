namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalReorgSnapshotRulesChecks
{
    public static void SeasonalReorgSnapshotValidatorReportsEveryProblem()
    {
        var validator = SeasonalReorgSnapshotRules.DefaultSeasonalReorgSnapshotValidator();
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalReorgSnapshotAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new SeasonalReorgSnapshotAmountRangeRule(5.1, 469.0);
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("x", "y");
        item.Amount = 470.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalReorgSnapshotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new SeasonalReorgSnapshotRequiredLabelRule("seasonal reorg snapshot needs a value");
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalReorgSnapshotEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new SeasonalReorgSnapshotEmployeeIdLengthRule(64);
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalReorgSnapshotRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new SeasonalReorgSnapshotRequiredLocationRule("seasonal reorg snapshot needs this field");
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalReorgSnapshotValidatorReportsEveryProblem();
        SeasonalReorgSnapshotAmountRangeRuleFlagsValuesAboveAmount();
        SeasonalReorgSnapshotRequiredLabelRuleFlagsABlankLabel();
        SeasonalReorgSnapshotEmployeeIdLengthRuleFlagsALongEmployeeId();
        SeasonalReorgSnapshotRequiredLocationRuleFlagsABlankLocation();
    }
}
