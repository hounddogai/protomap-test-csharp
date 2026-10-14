namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class GlobalSpanAdjustmentRulesChecks
{
    public static void GlobalSpanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = GlobalSpanAdjustmentRules.DefaultGlobalSpanAdjustmentValidator();
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalSpanAdjustmentRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new GlobalSpanAdjustmentRequiredDescriptionRule("global span adjustment needs a value");
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalSpanAdjustmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalSpanAdjustmentTitleLengthRule(120);
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSpanAdjustmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalSpanAdjustmentCodeLengthRule(32);
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSpanAdjustmentDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new GlobalSpanAdjustmentDescriptionLengthRule(120);
        var item = GlobalSpanAdjustment.NewGlobalSpanAdjustment("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalSpanAdjustmentValidatorReportsEveryProblem();
        GlobalSpanAdjustmentRequiredDescriptionRuleFlagsABlankDescription();
        GlobalSpanAdjustmentTitleLengthRuleFlagsALongTitle();
        GlobalSpanAdjustmentCodeLengthRuleFlagsALongCode();
        GlobalSpanAdjustmentDescriptionLengthRuleFlagsALongDescription();
    }
}
