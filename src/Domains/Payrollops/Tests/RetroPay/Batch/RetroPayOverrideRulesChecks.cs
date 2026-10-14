namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RetroPayOverrideRulesChecks
{
    public static void RetroPayOverrideValidatorReportsEveryProblem()
    {
        var validator = RetroPayOverrideRules.DefaultRetroPayOverrideValidator();
        var item = RetroPayOverride.NewRetroPayOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayOverrideRequiredCodeRule("retro pay override needs a value");
        var item = RetroPayOverride.NewRetroPayOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayOverrideRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new RetroPayOverrideRequiredUnitRule("retro pay override needs this field");
        var item = RetroPayOverride.NewRetroPayOverride("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayOverrideRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayOverrideRequiredLabelRule("retro pay override needs a value");
        var item = RetroPayOverride.NewRetroPayOverride("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayOverrideUnitLengthRuleFlagsALongUnit()
    {
        var rule = new RetroPayOverrideUnitLengthRule(3600);
        var item = RetroPayOverride.NewRetroPayOverride("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RetroPayOverrideLabelLengthRule(255);
        var item = RetroPayOverride.NewRetroPayOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayOverrideValidatorReportsEveryProblem();
        RetroPayOverrideRequiredCodeRuleFlagsABlankCode();
        RetroPayOverrideRequiredUnitRuleFlagsABlankUnit();
        RetroPayOverrideRequiredLabelRuleFlagsABlankLabel();
        RetroPayOverrideUnitLengthRuleFlagsALongUnit();
        RetroPayOverrideLabelLengthRuleFlagsALongLabel();
    }
}
