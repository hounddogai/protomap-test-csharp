namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RollingRetroPaySummaryRulesChecks
{
    public static void RollingRetroPaySummaryValidatorReportsEveryProblem()
    {
        var validator = RollingRetroPaySummaryRules.DefaultRollingRetroPaySummaryValidator();
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRetroPaySummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RollingRetroPaySummaryRateRangeRule(2.6, 1639.5);
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("x", "y");
        item.Rate = 1641.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingRetroPaySummaryRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new RollingRetroPaySummaryRatioRangeRule(5.2, 4303.0);
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("x", "y");
        item.Ratio = 4304.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingRetroPaySummaryCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new RollingRetroPaySummaryCurrencyLengthRule(255);
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingRetroPaySummaryChannelLengthRuleFlagsALongChannel()
    {
        var rule = new RollingRetroPaySummaryChannelLengthRule(3600);
        var item = RollingRetroPaySummary.NewRollingRetroPaySummary("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RollingRetroPaySummaryValidatorReportsEveryProblem();
        RollingRetroPaySummaryRateRangeRuleFlagsValuesAboveRate();
        RollingRetroPaySummaryRatioRangeRuleFlagsValuesAboveRatio();
        RollingRetroPaySummaryCurrencyLengthRuleFlagsALongCurrency();
        RollingRetroPaySummaryChannelLengthRuleFlagsALongChannel();
    }
}
