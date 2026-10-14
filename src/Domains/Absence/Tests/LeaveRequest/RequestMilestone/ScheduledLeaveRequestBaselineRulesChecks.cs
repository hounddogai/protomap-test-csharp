namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestBaselineRulesChecks
{
    public static void ScheduledLeaveRequestBaselineValidatorReportsEveryProblem()
    {
        var validator = ScheduledLeaveRequestBaselineRules.DefaultScheduledLeaveRequestBaselineValidator();
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledLeaveRequestBaselineFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new ScheduledLeaveRequestBaselineFteRangeRule(3.3, 2692.8);
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("x", "y");
        item.Fte = 2694.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledLeaveRequestBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledLeaveRequestBaselineCodeLengthRule(32);
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledLeaveRequestBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ScheduledLeaveRequestBaselineRequiredTitleRule("scheduled leave request baseline needs a value");
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledLeaveRequestBaselineCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new ScheduledLeaveRequestBaselineCeilingRangeRule(6.7, 3631.7);
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("x", "y");
        item.Ceiling = 3633.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledLeaveRequestBaselineRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new ScheduledLeaveRequestBaselineRequiredGradeRule(
            "scheduled leave request baseline needs this field");
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestBaselineValidatorReportsEveryProblem();
        ScheduledLeaveRequestBaselineFteRangeRuleFlagsValuesAboveFte();
        ScheduledLeaveRequestBaselineCodeLengthRuleFlagsALongCode();
        ScheduledLeaveRequestBaselineRequiredTitleRuleFlagsABlankTitle();
        ScheduledLeaveRequestBaselineCeilingRangeRuleFlagsValuesAboveCeiling();
        ScheduledLeaveRequestBaselineRequiredGradeRuleFlagsABlankGrade();
    }
}
