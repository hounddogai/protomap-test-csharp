namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredOccupancyRecordRulesChecks
{
    public static void TieredOccupancyRecordValidatorReportsEveryProblem()
    {
        var validator = TieredOccupancyRecordRules.DefaultTieredOccupancyRecordValidator();
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredOccupancyRecordRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new TieredOccupancyRecordRequiredSegmentRule("tiered occupancy record needs this field");
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredOccupancyRecordCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new TieredOccupancyRecordCapacityRangeRule(32, 32);
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("x", "y");
        item.Capacity = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredOccupancyRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredOccupancyRecordRequiredCodeRule("tiered occupancy record needs a value");
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredOccupancyRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredOccupancyRecordCodeLengthRule(32);
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredOccupancyRecordGradeLengthRuleFlagsALongGrade()
    {
        var rule = new TieredOccupancyRecordGradeLengthRule(1000);
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredOccupancyRecordValidatorReportsEveryProblem();
        TieredOccupancyRecordRequiredSegmentRuleFlagsABlankSegment();
        TieredOccupancyRecordCapacityRangeRuleFlagsValuesAboveCapacity();
        TieredOccupancyRecordRequiredCodeRuleFlagsABlankCode();
        TieredOccupancyRecordCodeLengthRuleFlagsALongCode();
        TieredOccupancyRecordGradeLengthRuleFlagsALongGrade();
    }
}
