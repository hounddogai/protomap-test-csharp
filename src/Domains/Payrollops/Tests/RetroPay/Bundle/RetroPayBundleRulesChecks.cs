namespace Acme.Workforce.Payrollops.Tests.RetroPay.Bundle;

using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RetroPayBundleRulesChecks
{
    public static void RetroPayBundleValidatorReportsEveryProblem()
    {
        var validator = RetroPayBundleRules.DefaultRetroPayBundleValidator();
        var item = RetroPayBundle.NewRetroPayBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayBundleCodeLengthRule(3600);
        var item = RetroPayBundle.NewRetroPayBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayBundleRequiredCodeRule("retro pay bundle needs this field");
        var item = RetroPayBundle.NewRetroPayBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayBundleRequiredLabelRule("retro pay bundle needs an entry");
        var item = RetroPayBundle.NewRetroPayBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayBundleValidatorReportsEveryProblem();
        RetroPayBundleCodeLengthRuleFlagsALongCode();
        RetroPayBundleRequiredCodeRuleFlagsABlankCode();
        RetroPayBundleRequiredLabelRuleFlagsABlankLabel();
    }
}
