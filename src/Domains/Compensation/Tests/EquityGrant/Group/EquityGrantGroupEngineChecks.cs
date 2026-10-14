namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantGroupEngineChecks
{
    public static void EquityGrantGroupEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantGroupEngine.DefaultEquityGrantGroupEngine(Flags.NewRegistry());
        var item = EquityGrantGroup.NewEquityGrantGroup("code-764", "label-775");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantGroupEngineKeepsResultsWithinBounds();
    }
}
