namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedSpanRuleChecks
{
    public static void CappedSpanRuleStartsInIdle()
    {
        var item = CappedSpanRule.NewCappedSpanRule("code-994", "label-93");
        Check.Equal(item.Status, CappedSpanRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSpanRuleMovesToWarming()
    {
        var item = CappedSpanRule.NewCappedSpanRule("code-994", "label-93");
        try
        {
            item.MoveTo(CappedSpanRuleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSpanRuleStatus.Warming, "status");
    }

    public static void CappedSpanRuleRejectsMovesFromStopped()
    {
        var item = CappedSpanRule.NewCappedSpanRule("code-994", "label-93");
        item.Status = CappedSpanRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedSpanRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedSpanRuleStartsInIdle();
        CappedSpanRuleMovesToWarming();
        CappedSpanRuleRejectsMovesFromStopped();
    }
}
