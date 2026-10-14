namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class CappedSpanTierChecks
{
    public static void CappedSpanTierStartsInIdle()
    {
        var item = CappedSpanTier.NewCappedSpanTier("code-808", "label-969");
        Check.Equal(item.Status, CappedSpanTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSpanTierMovesToWarming()
    {
        var item = CappedSpanTier.NewCappedSpanTier("code-808", "label-969");
        try
        {
            item.MoveTo(CappedSpanTierStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSpanTierStatus.Warming, "status");
    }

    public static void CappedSpanTierRejectsMovesFromStopped()
    {
        var item = CappedSpanTier.NewCappedSpanTier("code-808", "label-969");
        item.Status = CappedSpanTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedSpanTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedSpanTierStartsInIdle();
        CappedSpanTierMovesToWarming();
        CappedSpanTierRejectsMovesFromStopped();
    }
}
