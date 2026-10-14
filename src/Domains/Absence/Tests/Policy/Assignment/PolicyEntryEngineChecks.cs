namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyEntryEngineChecks
{
    public static void PolicyEntryEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyEntryEngine.DefaultPolicyEntryEngine();
        var item = PolicyEntry.NewPolicyEntry("code-103", "name-270");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyEntryEngineKeepsResultsWithinBounds();
    }
}
