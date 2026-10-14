namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedOffboardingBaselineChecks
{
    public static void CappedOffboardingBaselineStartsInDraft()
    {
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("code-502", "title-116");
        Check.Equal(item.Status, CappedOffboardingBaselineStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedOffboardingBaselineMovesToSuspended()
    {
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("code-502", "title-116");
        try
        {
            item.MoveTo(CappedOffboardingBaselineStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedOffboardingBaselineStatus.Suspended, "status");
    }

    public static void CappedOffboardingBaselineRejectsMovesFromClosed()
    {
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("code-502", "title-116");
        item.Status = CappedOffboardingBaselineStatus.Closed;
        Check.Fails(() => item.MoveTo(CappedOffboardingBaselineStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CappedOffboardingBaselineStartsInDraft();
        CappedOffboardingBaselineMovesToSuspended();
        CappedOffboardingBaselineRejectsMovesFromClosed();
    }
}
