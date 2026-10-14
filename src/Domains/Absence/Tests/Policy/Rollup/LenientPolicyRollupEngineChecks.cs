namespace Acme.Workforce.Absence.Tests.Policy.Rollup;

using Acme.Workforce.Absence.Policy.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientPolicyRollupEngineChecks
{
    public static void LenientPolicyRollupEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPolicyRollupEngine.DefaultLenientPolicyRollupEngine(Flags.NewRegistry());
        var item = LenientPolicyRollup.NewLenientPolicyRollup("code-773", "name-570");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPolicyRollupEngineKeepsResultsWithinBounds();
    }
}
