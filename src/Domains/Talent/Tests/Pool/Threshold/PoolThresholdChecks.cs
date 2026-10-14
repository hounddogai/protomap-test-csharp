namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolThresholdChecks
{
    public static void PoolThresholdStartsInCreated()
    {
        var item = PoolThreshold.NewPoolThreshold("code-954", "name-969");
        Check.Equal(item.Status, PoolThresholdStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolThresholdMovesToRetired()
    {
        var item = PoolThreshold.NewPoolThreshold("code-954", "name-969");
        try
        {
            item.MoveTo(PoolThresholdStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolThresholdStatus.Retired, "status");
    }

    public static void PoolThresholdRejectsMovesFromRetired()
    {
        var item = PoolThreshold.NewPoolThreshold("code-954", "name-969");
        item.Status = PoolThresholdStatus.Retired;
        Check.Fails(() => item.MoveTo(PoolThresholdStatus.Created), "move");
    }

    public static void RunAll()
    {
        PoolThresholdStartsInCreated();
        PoolThresholdMovesToRetired();
        PoolThresholdRejectsMovesFromRetired();
    }
}
