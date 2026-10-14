namespace Acme.Workforce.Talent.Tests.Nomination.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class NominationAssignmentEngineChecks
{
    public static void NominationAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = NominationAssignmentEngine.DefaultNominationAssignmentEngine(Flags.NewRegistry());
        var item = NominationAssignment.NewNominationAssignment("code-441", "label-121");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationAssignmentEngineKeepsResultsWithinBounds();
    }
}
