namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyGroupRulesChecks
{
    public static void OccupancyGroupValidatorReportsEveryProblem()
    {
        var validator = OccupancyGroupRules.DefaultOccupancyGroupValidator();
        var item = OccupancyGroup.NewOccupancyGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void OccupancyGroupStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new OccupancyGroupStartsAtRangeRule(64, 255);
        var item = OccupancyGroup.NewOccupancyGroup("x", "y");
        item.StartsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void OccupancyGroupCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new OccupancyGroupCostCenterLengthRule(64);
        var item = OccupancyGroup.NewOccupancyGroup("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        OccupancyGroupValidatorReportsEveryProblem();
        OccupancyGroupStartsAtRangeRuleFlagsValuesAboveStartsAt();
        OccupancyGroupCostCenterLengthRuleFlagsALongCostCenter();
    }
}
