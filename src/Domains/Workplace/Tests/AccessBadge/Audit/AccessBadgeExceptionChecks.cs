namespace Acme.Workforce.Workplace.Tests.AccessBadge.Audit;

using Acme.Workforce.Workplace.AccessBadge.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AccessBadgeExceptionChecks
{
    public static void AccessBadgeExceptionStartsInDraft()
    {
        var item = AccessBadgeException.NewAccessBadgeException("code-851", "title-363");
        Check.Equal(item.Status, AccessBadgeExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeExceptionMovesToSuspended()
    {
        var item = AccessBadgeException.NewAccessBadgeException("code-851", "title-363");
        try
        {
            item.MoveTo(AccessBadgeExceptionStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeExceptionStatus.Suspended, "status");
    }

    public static void AccessBadgeExceptionRejectsMovesFromClosed()
    {
        var item = AccessBadgeException.NewAccessBadgeException("code-851", "title-363");
        item.Status = AccessBadgeExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeExceptionStartsInDraft();
        AccessBadgeExceptionMovesToSuspended();
        AccessBadgeExceptionRejectsMovesFromClosed();
    }
}
