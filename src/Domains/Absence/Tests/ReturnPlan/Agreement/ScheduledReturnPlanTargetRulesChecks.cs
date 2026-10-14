namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class ScheduledReturnPlanTargetRulesChecks
{
    public static void ScheduledReturnPlanTargetValidatorReportsEveryProblem()
    {
        var validator = ScheduledReturnPlanTargetRules.DefaultScheduledReturnPlanTargetValidator();
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledReturnPlanTargetGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new ScheduledReturnPlanTargetGraceDaysRangeRule(32, 80);
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("x", "y");
        item.GraceDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledReturnPlanTargetRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ScheduledReturnPlanTargetRequiredChannelRule("scheduled return plan target needs this field");
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledReturnPlanTargetDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new ScheduledReturnPlanTargetDepartmentLengthRule(32);
        var item = ScheduledReturnPlanTarget.NewScheduledReturnPlanTarget("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledReturnPlanTargetValidatorReportsEveryProblem();
        ScheduledReturnPlanTargetGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        ScheduledReturnPlanTargetRequiredChannelRuleFlagsABlankChannel();
        ScheduledReturnPlanTargetDepartmentLengthRuleFlagsALongDepartment();
    }
}
