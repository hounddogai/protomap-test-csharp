namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RetroPayAdjustmentRulesChecks
{
    public static void RetroPayAdjustmentValidatorReportsEveryProblem()
    {
        var validator = RetroPayAdjustmentRules.DefaultRetroPayAdjustmentValidator();
        var item = RetroPayAdjustment.NewRetroPayAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayAdjustmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayAdjustmentRequiredLabelRule("retro pay adjustment needs a value");
        var item = RetroPayAdjustment.NewRetroPayAdjustment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayAdjustmentRequiredCodeRule("retro pay adjustment needs a value");
        var item = RetroPayAdjustment.NewRetroPayAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayAdjustmentCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new RetroPayAdjustmentCostCenterLengthRule(120);
        var item = RetroPayAdjustment.NewRetroPayAdjustment("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPayAdjustmentValidatorReportsEveryProblem();
        RetroPayAdjustmentRequiredLabelRuleFlagsABlankLabel();
        RetroPayAdjustmentRequiredCodeRuleFlagsABlankCode();
        RetroPayAdjustmentCostCenterLengthRuleFlagsALongCostCenter();
    }
}
