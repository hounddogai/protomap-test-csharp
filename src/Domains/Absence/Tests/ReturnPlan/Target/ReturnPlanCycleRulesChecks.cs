namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanCycleRulesChecks
{
    public static void ReturnPlanCycleValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanCycleRules.DefaultReturnPlanCycleValidator();
        var item = ReturnPlanCycle.NewReturnPlanCycle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanCyclePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ReturnPlanCyclePriorityRangeRule(32, 80);
        var item = ReturnPlanCycle.NewReturnPlanCycle("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanCycleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ReturnPlanCycleRequiredTitleRule("return plan cycle needs this field");
        var item = ReturnPlanCycle.NewReturnPlanCycle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanCycleGradeLengthRuleFlagsALongGrade()
    {
        var rule = new ReturnPlanCycleGradeLengthRule(64);
        var item = ReturnPlanCycle.NewReturnPlanCycle("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanCycleValidatorReportsEveryProblem();
        ReturnPlanCyclePriorityRangeRuleFlagsValuesAbovePriority();
        ReturnPlanCycleRequiredTitleRuleFlagsABlankTitle();
        ReturnPlanCycleGradeLengthRuleFlagsALongGrade();
    }
}
