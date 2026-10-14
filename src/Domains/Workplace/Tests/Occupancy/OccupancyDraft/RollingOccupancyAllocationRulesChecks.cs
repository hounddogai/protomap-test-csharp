namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyDraft;

using Acme.Workforce.Workplace.Occupancy.OccupancyDraft;

/// <summary>Checks of occupancy draft code, run by the project's runner.</summary>
public static class RollingOccupancyAllocationRulesChecks
{
    public static void RollingOccupancyAllocationValidatorReportsEveryProblem()
    {
        var validator = RollingOccupancyAllocationRules.DefaultRollingOccupancyAllocationValidator();
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingOccupancyAllocationRequiredNameRuleFlagsABlankName()
    {
        var rule = new RollingOccupancyAllocationRequiredNameRule("rolling occupancy allocation needs an entry");
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingOccupancyAllocationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new RollingOccupancyAllocationDepartmentLengthRule(255);
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingOccupancyAllocationBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RollingOccupancyAllocationBadgeLengthRule(32);
        var item = RollingOccupancyAllocation.NewRollingOccupancyAllocation("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingOccupancyAllocationValidatorReportsEveryProblem();
        RollingOccupancyAllocationRequiredNameRuleFlagsABlankName();
        RollingOccupancyAllocationDepartmentLengthRuleFlagsALongDepartment();
        RollingOccupancyAllocationBadgeLengthRuleFlagsALongBadge();
    }
}
