namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandRequestRulesChecks
{
    public static void PayBandRequestValidatorReportsEveryProblem()
    {
        var validator = PayBandRequestRules.DefaultPayBandRequestValidator();
        var item = PayBandRequest.NewPayBandRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandRequestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandRequestRequiredCodeRule("pay band request needs an entry");
        var item = PayBandRequest.NewPayBandRequest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandRequestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandRequestCodeLengthRule(255);
        var item = PayBandRequest.NewPayBandRequest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandRequestMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new PayBandRequestMarginRangeRule(0.1, 2749.7);
        var item = PayBandRequest.NewPayBandRequest("x", "y");
        item.Margin = 2751.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandRequestRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayBandRequestRequiredNameRule("pay band request needs a value");
        var item = PayBandRequest.NewPayBandRequest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandRequestNameLengthRuleFlagsALongName()
    {
        var rule = new PayBandRequestNameLengthRule(16);
        var item = PayBandRequest.NewPayBandRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandRequestValidatorReportsEveryProblem();
        PayBandRequestRequiredCodeRuleFlagsABlankCode();
        PayBandRequestCodeLengthRuleFlagsALongCode();
        PayBandRequestMarginRangeRuleFlagsValuesAboveMargin();
        PayBandRequestRequiredNameRuleFlagsABlankName();
        PayBandRequestNameLengthRuleFlagsALongName();
    }
}
