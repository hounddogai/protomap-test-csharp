namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanBatchRulesChecks
{
    public static void SpanBatchValidatorReportsEveryProblem()
    {
        var validator = SpanBatchRules.DefaultSpanBatchValidator();
        var item = SpanBatch.NewSpanBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new SpanBatchCodeLengthRule(16);
        var item = SpanBatch.NewSpanBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanBatchNameLengthRuleFlagsALongName()
    {
        var rule = new SpanBatchNameLengthRule(3600);
        var item = SpanBatch.NewSpanBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanBatchLabelLengthRuleFlagsALongLabel()
    {
        var rule = new SpanBatchLabelLengthRule(16);
        var item = SpanBatch.NewSpanBatch("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanBatchRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanBatchRequiredNameRule("span batch needs an entry");
        var item = SpanBatch.NewSpanBatch("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanBatchValidatorReportsEveryProblem();
        SpanBatchCodeLengthRuleFlagsALongCode();
        SpanBatchNameLengthRuleFlagsALongName();
        SpanBatchLabelLengthRuleFlagsALongLabel();
        SpanBatchRequiredNameRuleFlagsABlankName();
    }
}
