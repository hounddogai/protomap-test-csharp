namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingDraft;

using Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

/// <summary>Checks of offboarding draft code, run by the project's runner.</summary>
public static class PendingOffboardingDraftChecks
{
    public static void PendingOffboardingDraftStartsInRequested()
    {
        var item = PendingOffboardingDraft.NewPendingOffboardingDraft("code-895", "label-32");
        Check.Equal(item.Status, PendingOffboardingDraftStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingOffboardingDraftMovesToRevoked()
    {
        var item = PendingOffboardingDraft.NewPendingOffboardingDraft("code-895", "label-32");
        try
        {
            item.MoveTo(PendingOffboardingDraftStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingOffboardingDraftStatus.Revoked, "status");
    }

    public static void PendingOffboardingDraftRejectsMovesFromExpired()
    {
        var item = PendingOffboardingDraft.NewPendingOffboardingDraft("code-895", "label-32");
        item.Status = PendingOffboardingDraftStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingOffboardingDraftStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingOffboardingDraftStartsInRequested();
        PendingOffboardingDraftMovesToRevoked();
        PendingOffboardingDraftRejectsMovesFromExpired();
    }
}
