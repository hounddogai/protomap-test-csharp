namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class CostModelDigestEngineChecks
{
    public static void CostModelDigestEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelDigestEngine.DefaultCostModelDigestEngine(Flags.NewRegistry());
        var item = CostModelDigest.NewCostModelDigest("code-532", "label-139");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelDigestEngineKeepsResultsWithinBounds();
    }
}
