namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgRollupEngineChecks
{
    public static void ReorgRollupEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgRollupEngine.DefaultReorgRollupEngine(Flags.NewRegistry());
        var item = ReorgRollup.NewReorgRollup("code-83", "name-133");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgRollupEngineKeepsResultsWithinBounds();
    }
}
