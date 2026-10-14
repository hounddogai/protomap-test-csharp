namespace Acme.Workforce.Engagement.Tests.Recognition.Baseline;

using Acme.Workforce.Engagement.Recognition.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class RollingRecognitionBaselineChecks
{
    public static void RollingRecognitionBaselineStartsInNew()
    {
        var item = RollingRecognitionBaseline.NewRollingRecognitionBaseline("code-652", "title-496");
        Check.Equal(item.Status, RollingRecognitionBaselineStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRecognitionBaselineMovesToRunning()
    {
        var item = RollingRecognitionBaseline.NewRollingRecognitionBaseline("code-652", "title-496");
        try
        {
            item.MoveTo(RollingRecognitionBaselineStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRecognitionBaselineStatus.Running, "status");
    }

    public static void RollingRecognitionBaselineRejectsMovesFromFailed()
    {
        var item = RollingRecognitionBaseline.NewRollingRecognitionBaseline("code-652", "title-496");
        item.Status = RollingRecognitionBaselineStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingRecognitionBaselineStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingRecognitionBaselineStartsInNew();
        RollingRecognitionBaselineMovesToRunning();
        RollingRecognitionBaselineRejectsMovesFromFailed();
    }
}
