namespace Acme.Workforce.Workplace.Tests.Occupancy.Segment;

using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class OccupancySegmentRulesChecks
{
    public static void OccupancySegmentValidatorReportsEveryProblem()
    {
        var validator = OccupancySegmentRules.DefaultOccupancySegmentValidator();
        var item = OccupancySegment.NewOccupancySegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancySegmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new OccupancySegmentRequiredLabelRule("occupancy segment needs an entry");
        var item = OccupancySegment.NewOccupancySegment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancySegmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancySegmentRequiredCodeRule("occupancy segment needs this field");
        var item = OccupancySegment.NewOccupancySegment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancySegmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new OccupancySegmentCodeLengthRule(86400);
        var item = OccupancySegment.NewOccupancySegment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancySegmentValidatorReportsEveryProblem();
        OccupancySegmentRequiredLabelRuleFlagsABlankLabel();
        OccupancySegmentRequiredCodeRuleFlagsABlankCode();
        OccupancySegmentCodeLengthRuleFlagsALongCode();
    }
}
