namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PulseSurveyAllocationChecks
{
    public static void PulseSurveyAllocationStartsInIdle()
    {
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("code-452", "label-506");
        Check.Equal(item.Status, PulseSurveyAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyAllocationMovesToStopped()
    {
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("code-452", "label-506");
        try
        {
            item.MoveTo(PulseSurveyAllocationStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyAllocationStatus.Stopped, "status");
    }

    public static void PulseSurveyAllocationRejectsMovesFromStopped()
    {
        var item = PulseSurveyAllocation.NewPulseSurveyAllocation("code-452", "label-506");
        item.Status = PulseSurveyAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(PulseSurveyAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PulseSurveyAllocationStartsInIdle();
        PulseSurveyAllocationMovesToStopped();
        PulseSurveyAllocationRejectsMovesFromStopped();
    }
}
