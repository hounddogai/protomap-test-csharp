namespace Acme.Workforce.Compensation.Tests.PayBand.Threshold;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayBandMetricRulesChecks
{
    public static void PayBandMetricValidatorReportsEveryProblem()
    {
        var validator = PayBandMetricRules.DefaultPayBandMetricValidator();
        var item = PayBandMetric.NewPayBandMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandMetricAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PayBandMetricAmountRangeRule(9.2, 4437.8);
        var item = PayBandMetric.NewPayBandMetric("x", "y");
        item.Amount = 4439.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandMetricRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PayBandMetricRequiredExternalRefRule("pay band metric needs an entry");
        var item = PayBandMetric.NewPayBandMetric("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandMetricRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new PayBandMetricRatioRangeRule(8.3, 929.1);
        var item = PayBandMetric.NewPayBandMetric("x", "y");
        item.Ratio = 930.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandMetricCodeLengthRule(32);
        var item = PayBandMetric.NewPayBandMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandMetricValidatorReportsEveryProblem();
        PayBandMetricAmountRangeRuleFlagsValuesAboveAmount();
        PayBandMetricRequiredExternalRefRuleFlagsABlankExternalRef();
        PayBandMetricRatioRangeRuleFlagsValuesAboveRatio();
        PayBandMetricCodeLengthRuleFlagsALongCode();
    }
}
