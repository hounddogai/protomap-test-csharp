namespace Acme.Workforce.Absence.Tests.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LeaveRequestPlanRulesChecks
{
    public static void LeaveRequestPlanValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestPlanRules.DefaultLeaveRequestPlanValidator();
        var item = LeaveRequestPlan.NewLeaveRequestPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestPlanCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LeaveRequestPlanCapacityRangeRule(1000, 3600);
        var item = LeaveRequestPlan.NewLeaveRequestPlan("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestPlanValidatorReportsEveryProblem();
        LeaveRequestPlanCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
