namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelNoticeEngineChecks
{
    public static void CostModelNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = CostModelNoticeEngine.DefaultCostModelNoticeEngine(Flags.NewRegistry());
        var item = CostModelNotice.NewCostModelNotice("code-465", "title-703");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CostModelNoticeEngineKeepsResultsWithinBounds();
    }
}
