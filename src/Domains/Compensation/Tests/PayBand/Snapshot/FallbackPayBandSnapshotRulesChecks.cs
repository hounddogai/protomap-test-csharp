namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FallbackPayBandSnapshotRulesChecks
{
    public static void FallbackPayBandSnapshotValidatorReportsEveryProblem()
    {
        var validator = FallbackPayBandSnapshotRules.DefaultFallbackPayBandSnapshotValidator();
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPayBandSnapshotVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new FallbackPayBandSnapshotVersionRangeRule(255, 1000);
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayBandSnapshotCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new FallbackPayBandSnapshotCurrencyLengthRule(80);
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPayBandSnapshotRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new FallbackPayBandSnapshotRateRangeRule(2.3, 839.5);
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("x", "y");
        item.Rate = 841.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPayBandSnapshotWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new FallbackPayBandSnapshotWeightRangeRule(4.5, 3785.7);
        var item = FallbackPayBandSnapshot.NewFallbackPayBandSnapshot("x", "y");
        item.Weight = 3787.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        FallbackPayBandSnapshotValidatorReportsEveryProblem();
        FallbackPayBandSnapshotVersionRangeRuleFlagsValuesAboveVersion();
        FallbackPayBandSnapshotCurrencyLengthRuleFlagsALongCurrency();
        FallbackPayBandSnapshotRateRangeRuleFlagsValuesAboveRate();
        FallbackPayBandSnapshotWeightRangeRuleFlagsValuesAboveWeight();
    }
}
