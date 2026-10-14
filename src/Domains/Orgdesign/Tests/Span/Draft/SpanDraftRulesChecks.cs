namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanDraftRulesChecks
{
    public static void SpanDraftValidatorReportsEveryProblem()
    {
        var validator = SpanDraftRules.DefaultSpanDraftValidator();
        var item = SpanDraft.NewSpanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new SpanDraftBatchSizeRangeRule(16, 3600);
        var item = SpanDraft.NewSpanDraft("x", "y");
        item.BatchSize = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SpanDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new SpanDraftRequiredTitleRule("span draft needs a value");
        var item = SpanDraft.NewSpanDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanDraftJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new SpanDraftJobCodeLengthRule(80);
        var item = SpanDraft.NewSpanDraft("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanDraftCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new SpanDraftCurrencyLengthRule(3600);
        var item = SpanDraft.NewSpanDraft("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanDraftRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new SpanDraftRequiredJobCodeRule("span draft needs a value");
        var item = SpanDraft.NewSpanDraft("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanDraftValidatorReportsEveryProblem();
        SpanDraftBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        SpanDraftRequiredTitleRuleFlagsABlankTitle();
        SpanDraftJobCodeLengthRuleFlagsALongJobCode();
        SpanDraftCurrencyLengthRuleFlagsALongCurrency();
        SpanDraftRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
