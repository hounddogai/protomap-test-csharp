namespace Acme.Workforce.Assets.Tests.Refresh.Policy;

using Acme.Workforce.Assets.Refresh.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientRefreshProfileChecks
{
    public static void LenientRefreshProfileStartsInIdle()
    {
        var item = LenientRefreshProfile.NewLenientRefreshProfile("code-464", "label-581");
        Check.Equal(item.Status, LenientRefreshProfileStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRefreshProfileMovesToWarming()
    {
        var item = LenientRefreshProfile.NewLenientRefreshProfile("code-464", "label-581");
        try
        {
            item.MoveTo(LenientRefreshProfileStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRefreshProfileStatus.Warming, "status");
    }

    public static void LenientRefreshProfileRejectsMovesFromStopped()
    {
        var item = LenientRefreshProfile.NewLenientRefreshProfile("code-464", "label-581");
        item.Status = LenientRefreshProfileStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientRefreshProfileStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientRefreshProfileStartsInIdle();
        LenientRefreshProfileMovesToWarming();
        LenientRefreshProfileRejectsMovesFromStopped();
    }
}
