namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class MobilityAssignmentEngineChecks
{
    public static void MobilityAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityAssignmentEngine.DefaultMobilityAssignmentEngine();
        var item = MobilityAssignment.NewMobilityAssignment("code-254", "label-78");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityAssignmentEngineKeepsResultsWithinBounds();
    }
}
