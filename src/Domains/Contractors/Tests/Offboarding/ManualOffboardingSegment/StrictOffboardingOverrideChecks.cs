namespace Acme.Workforce.Contractors.Tests.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

/// <summary>Checks of manual offboarding segment code, run by the project's runner.</summary>
public static class StrictOffboardingOverrideChecks
{
    public static void StrictOffboardingOverrideStartsInProposed()
    {
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("code-553", "title-73");
        Check.Equal(item.Status, StrictOffboardingOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictOffboardingOverrideMovesToRejected()
    {
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("code-553", "title-73");
        try
        {
            item.MoveTo(StrictOffboardingOverrideStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictOffboardingOverrideStatus.Rejected, "status");
    }

    public static void StrictOffboardingOverrideRejectsMovesFromWithdrawn()
    {
        var item = StrictOffboardingOverride.NewStrictOffboardingOverride("code-553", "title-73");
        item.Status = StrictOffboardingOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StrictOffboardingOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StrictOffboardingOverrideStartsInProposed();
        StrictOffboardingOverrideMovesToRejected();
        StrictOffboardingOverrideRejectsMovesFromWithdrawn();
    }
}
