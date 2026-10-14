namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanRuleChecks
{
    public static void SpanRuleStartsInRequested()
    {
        var item = SpanRule.NewSpanRule("code-203", "title-899");
        Check.Equal(item.Status, SpanRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanRuleMovesToExpired()
    {
        var item = SpanRule.NewSpanRule("code-203", "title-899");
        try
        {
            item.MoveTo(SpanRuleStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanRuleStatus.Expired, "status");
    }

    public static void SpanRuleRejectsMovesFromExpired()
    {
        var item = SpanRule.NewSpanRule("code-203", "title-899");
        item.Status = SpanRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(SpanRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SpanRuleStartsInRequested();
        SpanRuleMovesToExpired();
        SpanRuleRejectsMovesFromExpired();
    }
}
