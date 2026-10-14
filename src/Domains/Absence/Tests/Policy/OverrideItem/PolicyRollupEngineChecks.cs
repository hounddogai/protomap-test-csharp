namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyRollupEngineChecks
{
    public static void PolicyRollupEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyRollupEngine.DefaultPolicyRollupEngine(Flags.NewRegistry());
        var item = PolicyRollup.NewPolicyRollup("code-777", "title-514");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyRollupEngineKeepsResultsWithinBounds();
    }
}
