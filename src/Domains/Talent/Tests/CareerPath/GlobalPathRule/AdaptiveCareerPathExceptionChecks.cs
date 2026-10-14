namespace Acme.Workforce.Talent.Tests.CareerPath.GlobalPathRule;

using Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Checks of global path rule code, run by the project's runner.</summary>
public static class AdaptiveCareerPathExceptionChecks
{
    public static void AdaptiveCareerPathExceptionStartsInIdle()
    {
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("code-163", "label-535");
        Check.Equal(item.Status, AdaptiveCareerPathExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveCareerPathExceptionMovesToStopped()
    {
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("code-163", "label-535");
        try
        {
            item.MoveTo(AdaptiveCareerPathExceptionStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveCareerPathExceptionStatus.Stopped, "status");
    }

    public static void AdaptiveCareerPathExceptionRejectsMovesFromStopped()
    {
        var item = AdaptiveCareerPathException.NewAdaptiveCareerPathException("code-163", "label-535");
        item.Status = AdaptiveCareerPathExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveCareerPathExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathExceptionStartsInIdle();
        AdaptiveCareerPathExceptionMovesToStopped();
        AdaptiveCareerPathExceptionRejectsMovesFromStopped();
    }
}
