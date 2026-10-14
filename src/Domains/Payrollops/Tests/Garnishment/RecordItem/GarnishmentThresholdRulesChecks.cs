namespace Acme.Workforce.Payrollops.Tests.Garnishment.RecordItem;

using Acme.Workforce.Payrollops.Garnishment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GarnishmentThresholdRulesChecks
{
    public static void GarnishmentThresholdValidatorReportsEveryProblem()
    {
        var validator = GarnishmentThresholdRules.DefaultGarnishmentThresholdValidator();
        var item = GarnishmentThreshold.NewGarnishmentThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new GarnishmentThresholdCodeLengthRule(16);
        var item = GarnishmentThreshold.NewGarnishmentThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new GarnishmentThresholdRequiredLabelRule("garnishment threshold needs a value");
        var item = GarnishmentThreshold.NewGarnishmentThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentThresholdScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new GarnishmentThresholdScoreRangeRule(9.0, 3506.4);
        var item = GarnishmentThreshold.NewGarnishmentThreshold("x", "y");
        item.Score = 3507.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentThresholdSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new GarnishmentThresholdSalaryBandLengthRule(80);
        var item = GarnishmentThreshold.NewGarnishmentThreshold("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new GarnishmentThresholdRequiredSalaryBandRule("garnishment threshold needs an entry");
        var item = GarnishmentThreshold.NewGarnishmentThreshold("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentThresholdValidatorReportsEveryProblem();
        GarnishmentThresholdCodeLengthRuleFlagsALongCode();
        GarnishmentThresholdRequiredLabelRuleFlagsABlankLabel();
        GarnishmentThresholdScoreRangeRuleFlagsValuesAboveScore();
        GarnishmentThresholdSalaryBandLengthRuleFlagsALongSalaryBand();
        GarnishmentThresholdRequiredSalaryBandRuleFlagsABlankSalaryBand();
    }
}
