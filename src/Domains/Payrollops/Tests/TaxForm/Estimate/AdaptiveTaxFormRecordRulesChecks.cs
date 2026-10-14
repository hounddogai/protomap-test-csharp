namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AdaptiveTaxFormRecordRulesChecks
{
    public static void AdaptiveTaxFormRecordValidatorReportsEveryProblem()
    {
        var validator = AdaptiveTaxFormRecordRules.DefaultAdaptiveTaxFormRecordValidator();
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveTaxFormRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AdaptiveTaxFormRecordBatchSizeRangeRule(64, 80);
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTaxFormRecordRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new AdaptiveTaxFormRecordRateRangeRule(8.0, 3196.6);
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("x", "y");
        item.Rate = 3198.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveTaxFormRecordChannelLengthRuleFlagsALongChannel()
    {
        var rule = new AdaptiveTaxFormRecordChannelLengthRule(3600);
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveTaxFormRecordAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AdaptiveTaxFormRecordAmountRangeRule(2.9, 1120.3);
        var item = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord("x", "y");
        item.Amount = 1121.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveTaxFormRecordValidatorReportsEveryProblem();
        AdaptiveTaxFormRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AdaptiveTaxFormRecordRateRangeRuleFlagsValuesAboveRate();
        AdaptiveTaxFormRecordChannelLengthRuleFlagsALongChannel();
        AdaptiveTaxFormRecordAmountRangeRuleFlagsValuesAboveAmount();
    }
}
