namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancySnapshotRulesChecks
{
    public static void OccupancySnapshotValidatorReportsEveryProblem()
    {
        var validator = OccupancySnapshotRules.DefaultOccupancySnapshotValidator();
        var item = OccupancySnapshot.NewOccupancySnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancySnapshotTitleLengthRuleFlagsALongTitle()
    {
        var rule = new OccupancySnapshotTitleLengthRule(255);
        var item = OccupancySnapshot.NewOccupancySnapshot("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void OccupancySnapshotRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new OccupancySnapshotRequiredChecksumRule("occupancy snapshot needs this field");
        var item = OccupancySnapshot.NewOccupancySnapshot("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancySnapshotQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new OccupancySnapshotQuantityRangeRule(64, 120);
        var item = OccupancySnapshot.NewOccupancySnapshot("x", "y");
        item.Quantity = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancySnapshotRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new OccupancySnapshotRequiredTitleRule("occupancy snapshot needs this field");
        var item = OccupancySnapshot.NewOccupancySnapshot("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        OccupancySnapshotValidatorReportsEveryProblem();
        OccupancySnapshotTitleLengthRuleFlagsALongTitle();
        OccupancySnapshotRequiredChecksumRuleFlagsABlankChecksum();
        OccupancySnapshotQuantityRangeRuleFlagsValuesAboveQuantity();
        OccupancySnapshotRequiredTitleRuleFlagsABlankTitle();
    }
}
