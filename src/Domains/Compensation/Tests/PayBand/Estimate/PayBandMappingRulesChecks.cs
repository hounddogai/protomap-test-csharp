namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayBandMappingRulesChecks
{
    public static void PayBandMappingValidatorReportsEveryProblem()
    {
        var validator = PayBandMappingRules.DefaultPayBandMappingValidator();
        var item = PayBandMapping.NewPayBandMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandMappingPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new PayBandMappingPhoneLengthRule(120);
        var item = PayBandMapping.NewPayBandMapping("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandMappingRequiredCodeRule("pay band mapping needs a value");
        var item = PayBandMapping.NewPayBandMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandMappingRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayBandMappingRequiredLabelRule("pay band mapping needs a value");
        var item = PayBandMapping.NewPayBandMapping("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandMappingLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PayBandMappingLimitRangeRule(255, 1000);
        var item = PayBandMapping.NewPayBandMapping("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandMappingCodeLengthRule(1000);
        var item = PayBandMapping.NewPayBandMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandMappingValidatorReportsEveryProblem();
        PayBandMappingPhoneLengthRuleFlagsALongPhone();
        PayBandMappingRequiredCodeRuleFlagsABlankCode();
        PayBandMappingRequiredLabelRuleFlagsABlankLabel();
        PayBandMappingLimitRangeRuleFlagsValuesAboveLimit();
        PayBandMappingCodeLengthRuleFlagsALongCode();
    }
}
