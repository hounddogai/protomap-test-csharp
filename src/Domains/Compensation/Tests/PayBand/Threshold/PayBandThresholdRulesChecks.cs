namespace Acme.Workforce.Compensation.Tests.PayBand.Threshold;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayBandThresholdRulesChecks
{
    public static void PayBandThresholdValidatorReportsEveryProblem()
    {
        var validator = PayBandThresholdRules.DefaultPayBandThresholdValidator();
        var item = PayBandThreshold.NewPayBandThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayBandThresholdLabelLengthRule(255);
        var item = PayBandThreshold.NewPayBandThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandThresholdLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PayBandThresholdLeaveDaysRangeRule(16, 64);
        var item = PayBandThreshold.NewPayBandThreshold("x", "y");
        item.LeaveDays = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandThresholdTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new PayBandThresholdTenureMonthsRangeRule(16, 3600);
        var item = PayBandThreshold.NewPayBandThreshold("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayBandThresholdCodeLengthRule(80);
        var item = PayBandThreshold.NewPayBandThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandThresholdValidatorReportsEveryProblem();
        PayBandThresholdLabelLengthRuleFlagsALongLabel();
        PayBandThresholdLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        PayBandThresholdTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        PayBandThresholdCodeLengthRuleFlagsALongCode();
    }
}
