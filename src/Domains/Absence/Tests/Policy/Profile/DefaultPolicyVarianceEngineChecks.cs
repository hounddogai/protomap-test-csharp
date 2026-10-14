namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultPolicyVarianceEngineChecks
{
    public static void DefaultPolicyVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPolicyVarianceEngine.DefaultDefaultPolicyVarianceEngine(Flags.NewRegistry());
        var item = DefaultPolicyVariance.NewDefaultPolicyVariance("code-663", "label-130");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPolicyVarianceEngineKeepsResultsWithinBounds();
    }
}
