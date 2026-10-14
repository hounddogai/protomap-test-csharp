namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class OffboardingWindowChecks
{
    public static void OffboardingWindowStartsInPlanned()
    {
        var item = OffboardingWindow.NewOffboardingWindow("code-383", "title-748");
        Check.Equal(item.Status, OffboardingWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingWindowMovesToScheduled()
    {
        var item = OffboardingWindow.NewOffboardingWindow("code-383", "title-748");
        try
        {
            item.MoveTo(OffboardingWindowStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingWindowStatus.Scheduled, "status");
    }

    public static void OffboardingWindowRejectsMovesFromCancelled()
    {
        var item = OffboardingWindow.NewOffboardingWindow("code-383", "title-748");
        item.Status = OffboardingWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(OffboardingWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        OffboardingWindowStartsInPlanned();
        OffboardingWindowMovesToScheduled();
        OffboardingWindowRejectsMovesFromCancelled();
    }
}
