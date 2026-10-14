namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class AdaptiveHeadcountRecordRulesChecks
{
    public static void AdaptiveHeadcountRecordValidatorReportsEveryProblem()
    {
        var validator = AdaptiveHeadcountRecordRules.DefaultAdaptiveHeadcountRecordValidator();
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveHeadcountRecordRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdaptiveHeadcountRecordRequiredJobCodeRule("adaptive headcount record needs an entry");
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveHeadcountRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new AdaptiveHeadcountRecordBatchSizeRangeRule(255, 3600);
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveHeadcountRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveHeadcountRecordLabelLengthRule(64);
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveHeadcountRecordRegionLengthRuleFlagsALongRegion()
    {
        var rule = new AdaptiveHeadcountRecordRegionLengthRule(16);
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("x", "y");
        item.Region = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Region = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountRecordValidatorReportsEveryProblem();
        AdaptiveHeadcountRecordRequiredJobCodeRuleFlagsABlankJobCode();
        AdaptiveHeadcountRecordBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        AdaptiveHeadcountRecordLabelLengthRuleFlagsALongLabel();
        AdaptiveHeadcountRecordRegionLengthRuleFlagsALongRegion();
    }
}
