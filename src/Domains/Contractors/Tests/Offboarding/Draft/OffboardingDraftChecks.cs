namespace Acme.Workforce.Contractors.Tests.Offboarding.Draft;

using Acme.Workforce.Contractors.Offboarding.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OffboardingDraftChecks
{
    public static void OffboardingDraftStartsInNew()
    {
        var item = OffboardingDraft.NewOffboardingDraft("code-559", "name-550");
        Check.Equal(item.Status, OffboardingDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingDraftMovesToQueued()
    {
        var item = OffboardingDraft.NewOffboardingDraft("code-559", "name-550");
        try
        {
            item.MoveTo(OffboardingDraftStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingDraftStatus.Queued, "status");
    }

    public static void OffboardingDraftRejectsMovesFromFailed()
    {
        var item = OffboardingDraft.NewOffboardingDraft("code-559", "name-550");
        item.Status = OffboardingDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(OffboardingDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        OffboardingDraftStartsInNew();
        OffboardingDraftMovesToQueued();
        OffboardingDraftRejectsMovesFromFailed();
    }
}
