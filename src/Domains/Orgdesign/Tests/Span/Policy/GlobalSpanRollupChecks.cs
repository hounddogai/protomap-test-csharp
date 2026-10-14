namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalSpanRollupChecks
{
    public static void GlobalSpanRollupStartsInIdle()
    {
        var item = GlobalSpanRollup.NewGlobalSpanRollup("code-121", "label-512");
        Check.Equal(item.Status, GlobalSpanRollupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSpanRollupMovesToWarming()
    {
        var item = GlobalSpanRollup.NewGlobalSpanRollup("code-121", "label-512");
        try
        {
            item.MoveTo(GlobalSpanRollupStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSpanRollupStatus.Warming, "status");
    }

    public static void GlobalSpanRollupRejectsMovesFromStopped()
    {
        var item = GlobalSpanRollup.NewGlobalSpanRollup("code-121", "label-512");
        item.Status = GlobalSpanRollupStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalSpanRollupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalSpanRollupStartsInIdle();
        GlobalSpanRollupMovesToWarming();
        GlobalSpanRollupRejectsMovesFromStopped();
    }
}
