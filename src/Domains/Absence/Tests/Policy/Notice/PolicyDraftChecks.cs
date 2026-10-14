namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyDraftChecks
{
    public static void PolicyDraftStartsInRequested()
    {
        var item = PolicyDraft.NewPolicyDraft("code-367", "label-836");
        Check.Equal(item.Status, PolicyDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyDraftMovesToGranted()
    {
        var item = PolicyDraft.NewPolicyDraft("code-367", "label-836");
        try
        {
            item.MoveTo(PolicyDraftStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyDraftStatus.Granted, "status");
    }

    public static void PolicyDraftRejectsMovesFromExpired()
    {
        var item = PolicyDraft.NewPolicyDraft("code-367", "label-836");
        item.Status = PolicyDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(PolicyDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PolicyDraftStartsInRequested();
        PolicyDraftMovesToGranted();
        PolicyDraftRejectsMovesFromExpired();
    }
}
