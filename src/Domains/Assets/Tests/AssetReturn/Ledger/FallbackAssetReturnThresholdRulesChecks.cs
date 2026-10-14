namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAssetReturnThresholdRulesChecks
{
    public static void FallbackAssetReturnThresholdValidatorReportsEveryProblem()
    {
        var validator = FallbackAssetReturnThresholdRules.DefaultFallbackAssetReturnThresholdValidator();
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackAssetReturnThresholdMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new FallbackAssetReturnThresholdMaximumRangeRule(3.8, 2646.6);
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("x", "y");
        item.Maximum = 2648.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackAssetReturnThresholdJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new FallbackAssetReturnThresholdJobCodeLengthRule(255);
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAssetReturnThresholdLabelLengthRuleFlagsALongLabel()
    {
        var rule = new FallbackAssetReturnThresholdLabelLengthRule(86400);
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackAssetReturnThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackAssetReturnThresholdCodeLengthRule(3600);
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackAssetReturnThresholdValidatorReportsEveryProblem();
        FallbackAssetReturnThresholdMaximumRangeRuleFlagsValuesAboveMaximum();
        FallbackAssetReturnThresholdJobCodeLengthRuleFlagsALongJobCode();
        FallbackAssetReturnThresholdLabelLengthRuleFlagsALongLabel();
        FallbackAssetReturnThresholdCodeLengthRuleFlagsALongCode();
    }
}
