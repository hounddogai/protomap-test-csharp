namespace Acme.Workforce.Talent.Tests.Pool.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PoolNoticeEngineChecks
{
    public static void PoolNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = PoolNoticeEngine.DefaultPoolNoticeEngine(Flags.NewRegistry());
        var item = PoolNotice.NewPoolNotice("code-407", "label-999");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolNoticeEngineKeepsResultsWithinBounds();
    }
}
