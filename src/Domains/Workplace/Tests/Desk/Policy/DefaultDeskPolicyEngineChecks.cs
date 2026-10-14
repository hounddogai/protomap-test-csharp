namespace Acme.Workforce.Workplace.Tests.Desk.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultDeskPolicyEngineChecks
{
    public static void DefaultDeskPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultDeskPolicyEngine.DefaultDefaultDeskPolicyEngine(Flags.NewRegistry());
        var item = DefaultDeskPolicy.NewDefaultDeskPolicy("code-559", "title-308");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultDeskPolicyEngineKeepsResultsWithinBounds();
    }
}
