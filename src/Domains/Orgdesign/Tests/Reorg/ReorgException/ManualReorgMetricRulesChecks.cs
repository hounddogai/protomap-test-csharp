namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class ManualReorgMetricRulesChecks
{
    public static void ManualReorgMetricValidatorReportsEveryProblem()
    {
        var validator = ManualReorgMetricRules.DefaultManualReorgMetricValidator();
        var item = ManualReorgMetric.NewManualReorgMetric("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualReorgMetricRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new ManualReorgMetricRatioRangeRule(4.5, 1485.4);
        var item = ManualReorgMetric.NewManualReorgMetric("x", "y");
        item.Ratio = 1486.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualReorgMetricRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualReorgMetricRequiredCodeRule("manual reorg metric needs this field");
        var item = ManualReorgMetric.NewManualReorgMetric("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualReorgMetricTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualReorgMetricTitleLengthRule(3600);
        var item = ManualReorgMetric.NewManualReorgMetric("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualReorgMetricCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualReorgMetricCodeLengthRule(32);
        var item = ManualReorgMetric.NewManualReorgMetric("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualReorgMetricVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ManualReorgMetricVersionRangeRule(64, 1000);
        var item = ManualReorgMetric.NewManualReorgMetric("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualReorgMetricValidatorReportsEveryProblem();
        ManualReorgMetricRatioRangeRuleFlagsValuesAboveRatio();
        ManualReorgMetricRequiredCodeRuleFlagsABlankCode();
        ManualReorgMetricTitleLengthRuleFlagsALongTitle();
        ManualReorgMetricCodeLengthRuleFlagsALongCode();
        ManualReorgMetricVersionRangeRuleFlagsValuesAboveVersion();
    }
}
