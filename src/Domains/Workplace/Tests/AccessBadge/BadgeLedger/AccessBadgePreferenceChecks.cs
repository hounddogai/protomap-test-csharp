namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgePreferenceChecks
{
    public static void AccessBadgePreferenceStartsInDraft()
    {
        var item = AccessBadgePreference.NewAccessBadgePreference("code-246", "title-473");
        Check.Equal(item.Status, AccessBadgePreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgePreferenceMovesToSuspended()
    {
        var item = AccessBadgePreference.NewAccessBadgePreference("code-246", "title-473");
        try
        {
            item.MoveTo(AccessBadgePreferenceStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgePreferenceStatus.Suspended, "status");
    }

    public static void AccessBadgePreferenceRejectsMovesFromClosed()
    {
        var item = AccessBadgePreference.NewAccessBadgePreference("code-246", "title-473");
        item.Status = AccessBadgePreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgePreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgePreferenceStartsInDraft();
        AccessBadgePreferenceMovesToSuspended();
        AccessBadgePreferenceRejectsMovesFromClosed();
    }
}
