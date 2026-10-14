namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalPayRunGroupEngineChecks
{
    public static void RegionalPayRunGroupEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalPayRunGroupEngine.DefaultRegionalPayRunGroupEngine(Flags.NewRegistry());
        var item = RegionalPayRunGroup.NewRegionalPayRunGroup("code-923", "name-878");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalPayRunGroupEngineKeepsResultsWithinBounds();
    }
}
