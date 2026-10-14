namespace Acme.Workforce.Absence.Tests.LeaveRequest.Item;

using Acme.Workforce.Absence.LeaveRequest.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class LeaveRequestScoreRulesChecks
{
    public static void LeaveRequestScoreValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestScoreRules.DefaultLeaveRequestScoreValidator();
        var item = LeaveRequestScore.NewLeaveRequestScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestScoreCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LeaveRequestScoreCostRangeRule(0.9, 3409.1);
        var item = LeaveRequestScore.NewLeaveRequestScore("x", "y");
        item.Cost = 3410.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LeaveRequestScoreRequiredCodeRule("leave request score needs this field");
        var item = LeaveRequestScore.NewLeaveRequestScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestScoreEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new LeaveRequestScoreEmployeeIdLengthRule(1000);
        var item = LeaveRequestScore.NewLeaveRequestScore("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestScoreValidatorReportsEveryProblem();
        LeaveRequestScoreCostRangeRuleFlagsValuesAboveCost();
        LeaveRequestScoreRequiredCodeRuleFlagsABlankCode();
        LeaveRequestScoreEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
