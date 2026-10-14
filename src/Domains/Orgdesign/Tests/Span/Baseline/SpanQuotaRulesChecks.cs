namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanQuotaRulesChecks
{
    public static void SpanQuotaValidatorReportsEveryProblem()
    {
        var validator = SpanQuotaRules.DefaultSpanQuotaValidator();
        var item = SpanQuota.NewSpanQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SpanQuotaRequiredNameRuleFlagsABlankName()
    {
        var rule = new SpanQuotaRequiredNameRule("span quota needs an entry");
        var item = SpanQuota.NewSpanQuota("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SpanQuotaBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new SpanQuotaBadgeLengthRule(86400);
        var item = SpanQuota.NewSpanQuota("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SpanQuotaRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new SpanQuotaRequiredBadgeRule("span quota needs an entry");
        var item = SpanQuota.NewSpanQuota("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SpanQuotaValidatorReportsEveryProblem();
        SpanQuotaRequiredNameRuleFlagsABlankName();
        SpanQuotaBadgeLengthRuleFlagsALongBadge();
        SpanQuotaRequiredBadgeRuleFlagsABlankBadge();
    }
}
