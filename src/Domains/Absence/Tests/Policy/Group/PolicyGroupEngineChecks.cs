namespace Acme.Workforce.Absence.Tests.Policy.Group;

using Acme.Workforce.Absence.Policy.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class PolicyGroupEngineChecks
{
    public static void PolicyGroupEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyGroupEngine.DefaultPolicyGroupEngine(Flags.NewRegistry());
        var item = PolicyGroup.NewPolicyGroup("code-330", "name-771");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyGroupEngineKeepsResultsWithinBounds();
    }
}
