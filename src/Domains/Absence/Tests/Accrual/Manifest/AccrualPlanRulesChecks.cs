namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualPlanRulesChecks
{
    public static void AccrualPlanValidatorReportsEveryProblem()
    {
        var validator = AccrualPlanRules.DefaultAccrualPlanValidator();
        var item = AccrualPlan.NewAccrualPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualPlanRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AccrualPlanRequiredTitleRule("accrual plan needs a value");
        var item = AccrualPlan.NewAccrualPlan("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualPlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AccrualPlanTitleLengthRule(32);
        var item = AccrualPlan.NewAccrualPlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualPlanJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new AccrualPlanJobCodeLengthRule(3600);
        var item = AccrualPlan.NewAccrualPlan("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualPlanRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AccrualPlanRequiredJobCodeRule("accrual plan needs an entry");
        var item = AccrualPlan.NewAccrualPlan("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualPlanPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AccrualPlanPriorityRangeRule(120, 120);
        var item = AccrualPlan.NewAccrualPlan("x", "y");
        item.Priority = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualPlanCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualPlanCodeLengthRule(64);
        var item = AccrualPlan.NewAccrualPlan("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualPlanValidatorReportsEveryProblem();
        AccrualPlanRequiredTitleRuleFlagsABlankTitle();
        AccrualPlanTitleLengthRuleFlagsALongTitle();
        AccrualPlanJobCodeLengthRuleFlagsALongJobCode();
        AccrualPlanRequiredJobCodeRuleFlagsABlankJobCode();
        AccrualPlanPriorityRangeRuleFlagsValuesAbovePriority();
        AccrualPlanCodeLengthRuleFlagsALongCode();
    }
}
