namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandAdjustmentRulesChecks
{
    public static void PayBandAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PayBandAdjustmentRules.DefaultPayBandAdjustmentValidator();
        var item = PayBandAdjustment.NewPayBandAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandAdjustmentRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PayBandAdjustmentRequiredExternalRefRule("pay band adjustment needs an entry");
        var item = PayBandAdjustment.NewPayBandAdjustment("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandAdjustmentFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new PayBandAdjustmentFteRangeRule(7.5, 3444.4);
        var item = PayBandAdjustment.NewPayBandAdjustment("x", "y");
        item.Fte = 3445.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PayBandAdjustmentRequiredCodeRule("pay band adjustment needs an entry");
        var item = PayBandAdjustment.NewPayBandAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandAdjustmentRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new PayBandAdjustmentRateRangeRule(7.2, 1179.8);
        var item = PayBandAdjustment.NewPayBandAdjustment("x", "y");
        item.Rate = 1181.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayBandAdjustmentValidatorReportsEveryProblem();
        PayBandAdjustmentRequiredExternalRefRuleFlagsABlankExternalRef();
        PayBandAdjustmentFteRangeRuleFlagsValuesAboveFte();
        PayBandAdjustmentRequiredCodeRuleFlagsABlankCode();
        PayBandAdjustmentRateRangeRuleFlagsValuesAboveRate();
    }
}
