namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class GlobalSpanRollupEngineChecks
{
    public static void GlobalSpanRollupEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalSpanRollupEngine.DefaultGlobalSpanRollupEngine(Flags.NewRegistry());
        var item = GlobalSpanRollup.NewGlobalSpanRollup("code-121", "label-512");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalSpanRollupEngineKeepsResultsWithinBounds();
    }
}
