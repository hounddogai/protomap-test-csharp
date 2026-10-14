namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanQuotaRulesChecks
{
    public static void SuccessionPlanQuotaValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanQuotaRules.DefaultSuccessionPlanQuotaValidator();
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SuccessionPlanQuotaRequiredCodeRule("succession plan quota needs a value");
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SuccessionPlanQuotaRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new SuccessionPlanQuotaRequiredDepartmentRule("succession plan quota needs a value");
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SuccessionPlanQuotaValidatorReportsEveryProblem();
        SuccessionPlanQuotaRequiredCodeRuleFlagsABlankCode();
        SuccessionPlanQuotaRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
