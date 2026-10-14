namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

/// <summary>Checks of offboarding segment code, run by the project's runner.</summary>
public static class AdaptiveOffboardingLedgerChecks
{
    public static void AdaptiveOffboardingLedgerStartsInPlanned()
    {
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("code-126", "name-867");
        Check.Equal(item.Status, AdaptiveOffboardingLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveOffboardingLedgerMovesToInProgress()
    {
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("code-126", "name-867");
        try
        {
            item.MoveTo(AdaptiveOffboardingLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveOffboardingLedgerStatus.InProgress, "status");
    }

    public static void AdaptiveOffboardingLedgerRejectsMovesFromCancelled()
    {
        var item = AdaptiveOffboardingLedger.NewAdaptiveOffboardingLedger("code-126", "name-867");
        item.Status = AdaptiveOffboardingLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveOffboardingLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveOffboardingLedgerStartsInPlanned();
        AdaptiveOffboardingLedgerMovesToInProgress();
        AdaptiveOffboardingLedgerRejectsMovesFromCancelled();
    }
}
