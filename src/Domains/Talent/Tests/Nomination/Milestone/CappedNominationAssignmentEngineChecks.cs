namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class CappedNominationAssignmentEngineChecks
{
    public static void CappedNominationAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = CappedNominationAssignmentEngine.DefaultCappedNominationAssignmentEngine();
        var item = CappedNominationAssignment.NewCappedNominationAssignment("code-261", "name-641");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedNominationAssignmentEngineKeepsResultsWithinBounds();
    }
}
