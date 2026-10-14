namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountPolicyEngineChecks
{
    public static void HeadcountPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountPolicyEngine.DefaultHeadcountPolicyEngine(Flags.NewRegistry());
        var item = HeadcountPolicy.NewHeadcountPolicy("code-186", "label-93");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountPolicyEngineKeepsResultsWithinBounds();
    }
}
