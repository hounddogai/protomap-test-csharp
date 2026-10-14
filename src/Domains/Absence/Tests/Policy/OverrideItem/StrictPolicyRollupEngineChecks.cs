namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPolicyRollupEngineChecks
{
    public static void StrictPolicyRollupEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPolicyRollupEngine.DefaultStrictPolicyRollupEngine(Flags.NewRegistry());
        var item = StrictPolicyRollup.NewStrictPolicyRollup("code-53", "label-829");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPolicyRollupEngineKeepsResultsWithinBounds();
    }
}
