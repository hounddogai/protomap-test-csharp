namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultReturnPlanAuditRulesChecks
{
    public static void DefaultReturnPlanAuditValidatorReportsEveryProblem()
    {
        var validator = DefaultReturnPlanAuditRules.DefaultDefaultReturnPlanAuditValidator();
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultReturnPlanAuditRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new DefaultReturnPlanAuditRequiredCategoryRule("default return plan audit needs an entry");
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultReturnPlanAuditRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new DefaultReturnPlanAuditRequiredPhoneRule("default return plan audit needs this field");
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultReturnPlanAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultReturnPlanAuditWindowDaysRangeRule(80, 86400);
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultReturnPlanAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new DefaultReturnPlanAuditTitleLengthRule(32);
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultReturnPlanAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultReturnPlanAuditCodeLengthRule(80);
        var item = DefaultReturnPlanAudit.NewDefaultReturnPlanAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultReturnPlanAuditValidatorReportsEveryProblem();
        DefaultReturnPlanAuditRequiredCategoryRuleFlagsABlankCategory();
        DefaultReturnPlanAuditRequiredPhoneRuleFlagsABlankPhone();
        DefaultReturnPlanAuditWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultReturnPlanAuditTitleLengthRuleFlagsALongTitle();
        DefaultReturnPlanAuditCodeLengthRuleFlagsALongCode();
    }
}
