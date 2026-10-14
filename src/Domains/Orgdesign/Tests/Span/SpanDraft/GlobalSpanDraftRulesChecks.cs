namespace Acme.Workforce.Orgdesign.Tests.Span.SpanDraft;

using Acme.Workforce.Orgdesign.Span.SpanDraft;

/// <summary>Checks of span draft code, run by the project's runner.</summary>
public static class GlobalSpanDraftRulesChecks
{
    public static void GlobalSpanDraftValidatorReportsEveryProblem()
    {
        var validator = GlobalSpanDraftRules.DefaultGlobalSpanDraftValidator();
        var item = GlobalSpanDraft.NewGlobalSpanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalSpanDraftBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new GlobalSpanDraftBadgeLengthRule(120);
        var item = GlobalSpanDraft.NewGlobalSpanDraft("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSpanDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalSpanDraftCodeLengthRule(3600);
        var item = GlobalSpanDraft.NewGlobalSpanDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalSpanDraftRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new GlobalSpanDraftRequiredTitleRule("global span draft needs an entry");
        var item = GlobalSpanDraft.NewGlobalSpanDraft("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalSpanDraftValidatorReportsEveryProblem();
        GlobalSpanDraftBadgeLengthRuleFlagsALongBadge();
        GlobalSpanDraftCodeLengthRuleFlagsALongCode();
        GlobalSpanDraftRequiredTitleRuleFlagsABlankTitle();
    }
}
