namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathRollupEngineChecks
{
    public static void CareerPathRollupEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathRollupEngine.DefaultCareerPathRollupEngine(Flags.NewRegistry());
        var item = CareerPathRollup.NewCareerPathRollup("code-350", "label-459");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathRollupEngineKeepsResultsWithinBounds();
    }
}
