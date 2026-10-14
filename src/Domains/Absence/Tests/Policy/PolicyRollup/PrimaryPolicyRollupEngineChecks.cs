namespace Acme.Workforce.Absence.Tests.Policy.PolicyRollup;

using Acme.Workforce.Absence.Policy.PolicyRollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy rollup code, run by the project's runner.</summary>
public static class PrimaryPolicyRollupEngineChecks
{
    public static void PrimaryPolicyRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPolicyRollupEngine.DefaultPrimaryPolicyRollupEngine(Flags.NewRegistry());
        var item = PrimaryPolicyRollup.NewPrimaryPolicyRollup("code-999", "label-331");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPolicyRollupEngineKeepsResultsWithinBounds();
    }
}
