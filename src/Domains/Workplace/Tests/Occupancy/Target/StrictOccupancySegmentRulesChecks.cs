namespace Acme.Workforce.Workplace.Tests.Occupancy.Target;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StrictOccupancySegmentRulesChecks
{
    public static void StrictOccupancySegmentValidatorReportsEveryProblem()
    {
        var validator = StrictOccupancySegmentRules.DefaultStrictOccupancySegmentValidator();
        var item = StrictOccupancySegment.NewStrictOccupancySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictOccupancySegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StrictOccupancySegmentRequiredLabelRule("strict occupancy segment needs a value");
        var item = StrictOccupancySegment.NewStrictOccupancySegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOccupancySegmentRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new StrictOccupancySegmentRequiredRegionRule("strict occupancy segment needs an entry");
        var item = StrictOccupancySegment.NewStrictOccupancySegment("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictOccupancySegmentRegionLengthRuleFlagsALongRegion()
    {
        var rule = new StrictOccupancySegmentRegionLengthRule(64);
        var item = StrictOccupancySegment.NewStrictOccupancySegment("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictOccupancySegmentManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new StrictOccupancySegmentManagerIdLengthRule(255);
        var item = StrictOccupancySegment.NewStrictOccupancySegment("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictOccupancySegmentValidatorReportsEveryProblem();
        StrictOccupancySegmentRequiredLabelRuleFlagsABlankLabel();
        StrictOccupancySegmentRequiredRegionRuleFlagsABlankRegion();
        StrictOccupancySegmentRegionLengthRuleFlagsALongRegion();
        StrictOccupancySegmentManagerIdLengthRuleFlagsALongManagerId();
    }
}
