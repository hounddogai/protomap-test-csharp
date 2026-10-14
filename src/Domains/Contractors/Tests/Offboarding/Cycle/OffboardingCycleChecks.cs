namespace Acme.Workforce.Contractors.Tests.Offboarding.Cycle;

using Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class OffboardingCycleChecks
{
    public static void OffboardingCycleStartsInPlanned()
    {
        var item = OffboardingCycle.NewOffboardingCycle("code-776", "name-431");
        Check.Equal(item.Status, OffboardingCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingCycleMovesToScheduled()
    {
        var item = OffboardingCycle.NewOffboardingCycle("code-776", "name-431");
        try
        {
            item.MoveTo(OffboardingCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingCycleStatus.Scheduled, "status");
    }

    public static void OffboardingCycleRejectsMovesFromCancelled()
    {
        var item = OffboardingCycle.NewOffboardingCycle("code-776", "name-431");
        item.Status = OffboardingCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OffboardingCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OffboardingCycleStartsInPlanned();
        OffboardingCycleMovesToScheduled();
        OffboardingCycleRejectsMovesFromCancelled();
    }
}
