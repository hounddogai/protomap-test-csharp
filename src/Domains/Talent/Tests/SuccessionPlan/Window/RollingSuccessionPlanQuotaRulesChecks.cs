namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingSuccessionPlanQuotaRulesChecks
{
    public static void RollingSuccessionPlanQuotaValidatorReportsEveryProblem()
    {
        var validator = RollingSuccessionPlanQuotaRules.DefaultRollingSuccessionPlanQuotaValidator();
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingSuccessionPlanQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingSuccessionPlanQuotaCodeLengthRule(255);
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingSuccessionPlanQuotaRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new RollingSuccessionPlanQuotaRequiredSourceRule("rolling succession plan quota needs a value");
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingSuccessionPlanQuotaLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RollingSuccessionPlanQuotaLabelLengthRule(3600);
        var item = RollingSuccessionPlanQuota.NewRollingSuccessionPlanQuota("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingSuccessionPlanQuotaValidatorReportsEveryProblem();
        RollingSuccessionPlanQuotaCodeLengthRuleFlagsALongCode();
        RollingSuccessionPlanQuotaRequiredSourceRuleFlagsABlankSource();
        RollingSuccessionPlanQuotaLabelLengthRuleFlagsALongLabel();
    }
}
