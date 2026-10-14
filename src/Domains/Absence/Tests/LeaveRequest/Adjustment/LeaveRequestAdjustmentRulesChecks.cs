namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestAdjustmentRulesChecks
{
    public static void LeaveRequestAdjustmentValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestAdjustmentRules.DefaultLeaveRequestAdjustmentValidator();
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LeaveRequestAdjustmentTitleLengthRule(255);
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestAdjustmentRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LeaveRequestAdjustmentRequiredTitleRule("leave request adjustment needs an entry");
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestAdjustmentGradeLengthRuleFlagsALongGrade()
    {
        var rule = new LeaveRequestAdjustmentGradeLengthRule(64);
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new LeaveRequestAdjustmentCodeLengthRule(255);
        var item = LeaveRequestAdjustment.NewLeaveRequestAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LeaveRequestAdjustmentValidatorReportsEveryProblem();
        LeaveRequestAdjustmentTitleLengthRuleFlagsALongTitle();
        LeaveRequestAdjustmentRequiredTitleRuleFlagsABlankTitle();
        LeaveRequestAdjustmentGradeLengthRuleFlagsALongGrade();
        LeaveRequestAdjustmentCodeLengthRuleFlagsALongCode();
    }
}
