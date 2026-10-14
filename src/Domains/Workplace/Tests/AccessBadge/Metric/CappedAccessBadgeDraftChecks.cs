namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedAccessBadgeDraftChecks
{
    public static void CappedAccessBadgeDraftStartsInDraft()
    {
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("code-412", "label-655");
        Check.Equal(item.Status, CappedAccessBadgeDraftStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedAccessBadgeDraftMovesToActive()
    {
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("code-412", "label-655");
        try
        {
            item.MoveTo(CappedAccessBadgeDraftStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedAccessBadgeDraftStatus.Active, "status");
    }

    public static void CappedAccessBadgeDraftRejectsMovesFromClosed()
    {
        var item = CappedAccessBadgeDraft.NewCappedAccessBadgeDraft("code-412", "label-655");
        item.Status = CappedAccessBadgeDraftStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedAccessBadgeDraftStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedAccessBadgeDraftStartsInDraft();
        CappedAccessBadgeDraftMovesToActive();
        CappedAccessBadgeDraftRejectsMovesFromClosed();
    }
}
