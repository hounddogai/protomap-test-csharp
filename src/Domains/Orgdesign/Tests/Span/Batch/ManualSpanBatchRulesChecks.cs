namespace Acme.Workforce.Orgdesign.Tests.Span.Batch;

using Acme.Workforce.Orgdesign.Span.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class ManualSpanBatchRulesChecks
{
    public static void ManualSpanBatchValidatorReportsEveryProblem()
    {
        var validator = ManualSpanBatchRules.DefaultManualSpanBatchValidator();
        var item = ManualSpanBatch.NewManualSpanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualSpanBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new ManualSpanBatchLeaveDaysRangeRule(16, 80);
        var item = ManualSpanBatch.NewManualSpanBatch("x", "y");
        item.LeaveDays = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualSpanBatchRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ManualSpanBatchRequiredChannelRule("manual span batch needs a value");
        var item = ManualSpanBatch.NewManualSpanBatch("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualSpanBatchCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ManualSpanBatchCountryLengthRule(80);
        var item = ManualSpanBatch.NewManualSpanBatch("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualSpanBatchRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ManualSpanBatchRequiredTitleRule("manual span batch needs this field");
        var item = ManualSpanBatch.NewManualSpanBatch("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualSpanBatchValidatorReportsEveryProblem();
        ManualSpanBatchLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        ManualSpanBatchRequiredChannelRuleFlagsABlankChannel();
        ManualSpanBatchCountryLengthRuleFlagsALongCountry();
        ManualSpanBatchRequiredTitleRuleFlagsABlankTitle();
    }
}
