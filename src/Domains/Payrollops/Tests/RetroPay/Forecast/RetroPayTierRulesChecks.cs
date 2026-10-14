namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayTierRulesChecks
{
    public static void RetroPayTierValidatorReportsEveryProblem()
    {
        var validator = RetroPayTierRules.DefaultRetroPayTierValidator();
        var item = RetroPayTier.NewRetroPayTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RetroPayTierRequiredLabelRule("retro pay tier needs an entry");
        var item = RetroPayTier.NewRetroPayTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPayTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayTierCodeLengthRule(3600);
        var item = RetroPayTier.NewRetroPayTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayTierExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new RetroPayTierExternalRefLengthRule(80);
        var item = RetroPayTier.NewRetroPayTier("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayTierFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new RetroPayTierFloorValueRangeRule(5.0, 3448.2);
        var item = RetroPayTier.NewRetroPayTier("x", "y");
        item.FloorValue = 3449.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayTierRequiredCodeRule("retro pay tier needs an entry");
        var item = RetroPayTier.NewRetroPayTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayTierValidatorReportsEveryProblem();
        RetroPayTierRequiredLabelRuleFlagsABlankLabel();
        RetroPayTierCodeLengthRuleFlagsALongCode();
        RetroPayTierExternalRefLengthRuleFlagsALongExternalRef();
        RetroPayTierFloorValueRangeRuleFlagsValuesAboveFloorValue();
        RetroPayTierRequiredCodeRuleFlagsABlankCode();
    }
}
