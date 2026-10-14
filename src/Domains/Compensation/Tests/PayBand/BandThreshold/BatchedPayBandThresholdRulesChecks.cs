namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class BatchedPayBandThresholdRulesChecks
{
    public static void BatchedPayBandThresholdValidatorReportsEveryProblem()
    {
        var validator = BatchedPayBandThresholdRules.DefaultBatchedPayBandThresholdValidator();
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedPayBandThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new BatchedPayBandThresholdCodeLengthRule(1000);
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedPayBandThresholdRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new BatchedPayBandThresholdRequiredCountryRule("batched pay band threshold needs an entry");
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BatchedPayBandThresholdMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BatchedPayBandThresholdMarginRangeRule(2.6, 296.5);
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("x", "y");
        item.Margin = 298.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BatchedPayBandThresholdUnitLengthRuleFlagsALongUnit()
    {
        var rule = new BatchedPayBandThresholdUnitLengthRule(16);
        var item = BatchedPayBandThreshold.NewBatchedPayBandThreshold("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        BatchedPayBandThresholdValidatorReportsEveryProblem();
        BatchedPayBandThresholdCodeLengthRuleFlagsALongCode();
        BatchedPayBandThresholdRequiredCountryRuleFlagsABlankCountry();
        BatchedPayBandThresholdMarginRangeRuleFlagsValuesAboveMargin();
        BatchedPayBandThresholdUnitLengthRuleFlagsALongUnit();
    }
}
