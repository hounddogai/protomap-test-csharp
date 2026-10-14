namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalPositionPolicyEngineChecks
{
    public static void RegionalPositionPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalPositionPolicyEngine.DefaultRegionalPositionPolicyEngine(Flags.NewRegistry());
        var item = RegionalPositionPolicy.NewRegionalPositionPolicy("code-28", "label-225");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalPositionPolicyEngineKeepsResultsWithinBounds();
    }
}
