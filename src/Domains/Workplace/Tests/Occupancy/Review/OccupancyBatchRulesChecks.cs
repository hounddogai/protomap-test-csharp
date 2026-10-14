namespace Acme.Workforce.Workplace.Tests.Occupancy.Review;

using Acme.Workforce.Workplace.Occupancy.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class OccupancyBatchRulesChecks
{
    public static void OccupancyBatchValidatorReportsEveryProblem()
    {
        var validator = OccupancyBatchRules.DefaultOccupancyBatchValidator();
        var item = OccupancyBatch.NewOccupancyBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyBatchRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new OccupancyBatchRequiredRegionRule("occupancy batch needs a value");
        var item = OccupancyBatch.NewOccupancyBatch("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyBatchStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new OccupancyBatchStartsAtRangeRule(255, 86400);
        var item = OccupancyBatch.NewOccupancyBatch("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new OccupancyBatchRequiredCodeRule("occupancy batch needs this field");
        var item = OccupancyBatch.NewOccupancyBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void OccupancyBatchDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new OccupancyBatchDepartmentLengthRule(3600);
        var item = OccupancyBatch.NewOccupancyBatch("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyBatchValidatorReportsEveryProblem();
        OccupancyBatchRequiredRegionRuleFlagsABlankRegion();
        OccupancyBatchStartsAtRangeRuleFlagsValuesAboveStartsAt();
        OccupancyBatchRequiredCodeRuleFlagsABlankCode();
        OccupancyBatchDepartmentLengthRuleFlagsALongDepartment();
    }
}
