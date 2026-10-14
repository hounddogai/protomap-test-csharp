namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedSpanThresholdChecks
{
    public static void CappedSpanThresholdStartsInDraft()
    {
        var item = CappedSpanThreshold.NewCappedSpanThreshold("code-474", "title-349");
        Check.Equal(item.Status, CappedSpanThresholdStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedSpanThresholdMovesToActive()
    {
        var item = CappedSpanThreshold.NewCappedSpanThreshold("code-474", "title-349");
        try
        {
            item.MoveTo(CappedSpanThresholdStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedSpanThresholdStatus.Active, "status");
    }

    public static void CappedSpanThresholdRejectsMovesFromClosed()
    {
        var item = CappedSpanThreshold.NewCappedSpanThreshold("code-474", "title-349");
        item.Status = CappedSpanThresholdStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedSpanThresholdStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedSpanThresholdStartsInDraft();
        CappedSpanThresholdMovesToActive();
        CappedSpanThresholdRejectsMovesFromClosed();
    }
}
