namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledNominationExceptionEngineChecks
{
    public static void ScheduledNominationExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledNominationExceptionEngine.DefaultScheduledNominationExceptionEngine();
        var item = ScheduledNominationException.NewScheduledNominationException("code-939", "title-290");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledNominationExceptionEngineKeepsResultsWithinBounds();
    }
}
