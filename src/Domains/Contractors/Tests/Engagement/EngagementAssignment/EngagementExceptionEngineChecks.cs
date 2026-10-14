namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementAssignment;

using Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>Checks of engagement assignment code, run by the project's runner.</summary>
public static class EngagementExceptionEngineChecks
{
    public static void EngagementExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementExceptionEngine.DefaultEngagementExceptionEngine();
        var item = EngagementException.NewEngagementException("code-305", "title-668");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementExceptionEngineKeepsResultsWithinBounds();
    }
}
