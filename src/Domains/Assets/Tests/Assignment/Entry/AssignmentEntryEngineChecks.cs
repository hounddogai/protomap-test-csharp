namespace Acme.Workforce.Assets.Tests.Assignment.Entry;

using Acme.Workforce.Assets.Assignment.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AssignmentEntryEngineChecks
{
    public static void AssignmentEntryEngineKeepsResultsWithinBounds()
    {
        var engine = AssignmentEntryEngine.DefaultAssignmentEntryEngine(Flags.NewRegistry());
        var item = AssignmentEntry.NewAssignmentEntry("code-555", "name-182");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssignmentEntryEngineKeepsResultsWithinBounds();
    }
}
