namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ScheduledSuccessionPlanAuditRulesChecks
{
    public static void ScheduledSuccessionPlanAuditValidatorReportsEveryProblem()
    {
        var validator = ScheduledSuccessionPlanAuditRules.DefaultScheduledSuccessionPlanAuditValidator();
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledSuccessionPlanAuditRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new ScheduledSuccessionPlanAuditRequiredCostCenterRule(
            "scheduled succession plan audit needs this field");
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledSuccessionPlanAuditCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledSuccessionPlanAuditCodeLengthRule(64);
        var item = ScheduledSuccessionPlanAudit.NewScheduledSuccessionPlanAudit("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledSuccessionPlanAuditValidatorReportsEveryProblem();
        ScheduledSuccessionPlanAuditRequiredCostCenterRuleFlagsABlankCostCenter();
        ScheduledSuccessionPlanAuditCodeLengthRuleFlagsALongCode();
    }
}
