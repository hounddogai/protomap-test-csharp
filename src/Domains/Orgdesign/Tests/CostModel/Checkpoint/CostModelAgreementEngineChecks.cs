namespace Acme.Workforce.Orgdesign.Tests.CostModel.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class CostModelAgreementEngineChecks
{
    public static void CostModelAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelAgreementEngine.DefaultCostModelAgreementEngine(Flags.NewRegistry());
        var item = CostModelAgreement.NewCostModelAgreement("code-344", "name-143");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelAgreementEngineKeepsResultsWithinBounds();
    }
}
