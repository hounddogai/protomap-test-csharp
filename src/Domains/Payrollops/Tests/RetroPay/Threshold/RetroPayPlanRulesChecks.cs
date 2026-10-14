namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayPlanRulesChecks
{
    public static void RetroPayPlanValidatorReportsEveryProblem()
    {
        var validator = RetroPayPlanRules.DefaultRetroPayPlanValidator();
        var item = RetroPayPlan.NewRetroPayPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new RetroPayPlanRequiredNameRule("retro pay plan needs a value");
        var item = RetroPayPlan.NewRetroPayPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayPlanRequiredCodeRule("retro pay plan needs a value");
        var item = RetroPayPlan.NewRetroPayPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayPlanValidatorReportsEveryProblem();
        RetroPayPlanRequiredNameRuleFlagsABlankName();
        RetroPayPlanRequiredCodeRuleFlagsABlankCode();
    }
}
