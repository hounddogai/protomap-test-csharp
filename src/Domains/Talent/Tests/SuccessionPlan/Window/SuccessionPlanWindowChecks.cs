namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Window;

using Acme.Workforce.Talent.SuccessionPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class SuccessionPlanWindowChecks
{
    public static void SuccessionPlanWindowStartsInRequested()
    {
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("code-605", "label-213");
        Check.Equal(item.Status, SuccessionPlanWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanWindowMovesToGranted()
    {
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("code-605", "label-213");
        try
        {
            item.MoveTo(SuccessionPlanWindowStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanWindowStatus.Granted, "status");
    }

    public static void SuccessionPlanWindowRejectsMovesFromExpired()
    {
        var item = SuccessionPlanWindow.NewSuccessionPlanWindow("code-605", "label-213");
        item.Status = SuccessionPlanWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(SuccessionPlanWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanWindowStartsInRequested();
        SuccessionPlanWindowMovesToGranted();
        SuccessionPlanWindowRejectsMovesFromExpired();
    }
}
