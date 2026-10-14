namespace Acme.Workforce.Talent.Tests.Pool.Target;

using Acme.Workforce.Talent.Pool.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPoolTargetChecks
{
    public static void LegacyPoolTargetStartsInIdle()
    {
        var item = LegacyPoolTarget.NewLegacyPoolTarget("code-747", "name-634");
        Check.Equal(item.Status, LegacyPoolTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPoolTargetMovesToWarming()
    {
        var item = LegacyPoolTarget.NewLegacyPoolTarget("code-747", "name-634");
        try
        {
            item.MoveTo(LegacyPoolTargetStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPoolTargetStatus.Warming, "status");
    }

    public static void LegacyPoolTargetRejectsMovesFromStopped()
    {
        var item = LegacyPoolTarget.NewLegacyPoolTarget("code-747", "name-634");
        item.Status = LegacyPoolTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyPoolTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyPoolTargetStartsInIdle();
        LegacyPoolTargetMovesToWarming();
        LegacyPoolTargetRejectsMovesFromStopped();
    }
}
