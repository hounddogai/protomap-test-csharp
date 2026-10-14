namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PendingOffboardingCycleChecks
{
    public static void PendingOffboardingCycleStartsInOpen()
    {
        var item = PendingOffboardingCycle.NewPendingOffboardingCycle("code-981", "label-227");
        Check.Equal(item.Status, PendingOffboardingCycleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingOffboardingCycleMovesToReopened()
    {
        var item = PendingOffboardingCycle.NewPendingOffboardingCycle("code-981", "label-227");
        try
        {
            item.MoveTo(PendingOffboardingCycleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingOffboardingCycleStatus.Reopened, "status");
    }

    public static void PendingOffboardingCycleRejectsMovesFromArchived()
    {
        var item = PendingOffboardingCycle.NewPendingOffboardingCycle("code-981", "label-227");
        item.Status = PendingOffboardingCycleStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingOffboardingCycleStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingOffboardingCycleStartsInOpen();
        PendingOffboardingCycleMovesToReopened();
        PendingOffboardingCycleRejectsMovesFromArchived();
    }
}
