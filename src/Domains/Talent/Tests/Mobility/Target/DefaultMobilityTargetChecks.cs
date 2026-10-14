namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class DefaultMobilityTargetChecks
{
    public static void DefaultMobilityTargetStartsInIdle()
    {
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("code-498", "title-646");
        Check.Equal(item.Status, DefaultMobilityTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultMobilityTargetMovesToReady()
    {
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("code-498", "title-646");
        try
        {
            item.MoveTo(DefaultMobilityTargetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultMobilityTargetStatus.Ready, "status");
    }

    public static void DefaultMobilityTargetRejectsMovesFromStopped()
    {
        var item = DefaultMobilityTarget.NewDefaultMobilityTarget("code-498", "title-646");
        item.Status = DefaultMobilityTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(DefaultMobilityTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DefaultMobilityTargetStartsInIdle();
        DefaultMobilityTargetMovesToReady();
        DefaultMobilityTargetRejectsMovesFromStopped();
    }
}
