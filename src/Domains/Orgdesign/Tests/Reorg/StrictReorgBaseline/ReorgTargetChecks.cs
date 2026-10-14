namespace Acme.Workforce.Orgdesign.Tests.Reorg.StrictReorgBaseline;

using Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

/// <summary>Checks of strict reorg baseline code, run by the project's runner.</summary>
public static class ReorgTargetChecks
{
    public static void ReorgTargetStartsInIdle()
    {
        var item = ReorgTarget.NewReorgTarget("code-646", "title-572");
        Check.Equal(item.Status, ReorgTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgTargetMovesToWarming()
    {
        var item = ReorgTarget.NewReorgTarget("code-646", "title-572");
        try
        {
            item.MoveTo(ReorgTargetStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgTargetStatus.Warming, "status");
    }

    public static void ReorgTargetRejectsMovesFromStopped()
    {
        var item = ReorgTarget.NewReorgTarget("code-646", "title-572");
        item.Status = ReorgTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgTargetStartsInIdle();
        ReorgTargetMovesToWarming();
        ReorgTargetRejectsMovesFromStopped();
    }
}
