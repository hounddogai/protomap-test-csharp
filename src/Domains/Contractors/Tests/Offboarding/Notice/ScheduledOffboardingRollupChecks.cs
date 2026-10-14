namespace Acme.Workforce.Contractors.Tests.Offboarding.Notice;

using Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledOffboardingRollupChecks
{
    public static void ScheduledOffboardingRollupStartsInProposed()
    {
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("code-999", "title-963");
        Check.Equal(item.Status, ScheduledOffboardingRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledOffboardingRollupMovesToRejected()
    {
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("code-999", "title-963");
        try
        {
            item.MoveTo(ScheduledOffboardingRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledOffboardingRollupStatus.Rejected, "status");
    }

    public static void ScheduledOffboardingRollupRejectsMovesFromWithdrawn()
    {
        var item = ScheduledOffboardingRollup.NewScheduledOffboardingRollup("code-999", "title-963");
        item.Status = ScheduledOffboardingRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledOffboardingRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledOffboardingRollupStartsInProposed();
        ScheduledOffboardingRollupMovesToRejected();
        ScheduledOffboardingRollupRejectsMovesFromWithdrawn();
    }
}
