namespace Acme.Workforce.Contractors.Tests.Offboarding.Profile;

using Acme.Workforce.Contractors.Offboarding.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class OffboardingTargetChecks
{
    public static void OffboardingTargetStartsInPlanned()
    {
        var item = OffboardingTarget.NewOffboardingTarget("code-764", "title-35");
        Check.Equal(item.Status, OffboardingTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingTargetMovesToCancelled()
    {
        var item = OffboardingTarget.NewOffboardingTarget("code-764", "title-35");
        try
        {
            item.MoveTo(OffboardingTargetStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingTargetStatus.Cancelled, "status");
    }

    public static void OffboardingTargetRejectsMovesFromCancelled()
    {
        var item = OffboardingTarget.NewOffboardingTarget("code-764", "title-35");
        item.Status = OffboardingTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OffboardingTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OffboardingTargetStartsInPlanned();
        OffboardingTargetMovesToCancelled();
        OffboardingTargetRejectsMovesFromCancelled();
    }
}
