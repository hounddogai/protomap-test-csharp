namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptivePositionProfileChecks
{
    public static void AdaptivePositionProfileStartsInRequested()
    {
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("code-320", "title-450");
        Check.Equal(item.Status, AdaptivePositionProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptivePositionProfileMovesToGranted()
    {
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("code-320", "title-450");
        try
        {
            item.MoveTo(AdaptivePositionProfileStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptivePositionProfileStatus.Granted, "status");
    }

    public static void AdaptivePositionProfileRejectsMovesFromExpired()
    {
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("code-320", "title-450");
        item.Status = AdaptivePositionProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(AdaptivePositionProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        AdaptivePositionProfileStartsInRequested();
        AdaptivePositionProfileMovesToGranted();
        AdaptivePositionProfileRejectsMovesFromExpired();
    }
}
