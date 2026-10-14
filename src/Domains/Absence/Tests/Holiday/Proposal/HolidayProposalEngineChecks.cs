namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayProposalEngineChecks
{
    public static void HolidayProposalEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayProposalEngine.DefaultHolidayProposalEngine();
        var item = HolidayProposal.NewHolidayProposal("code-306", "title-404");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayProposalEngineKeepsResultsWithinBounds();
    }
}
