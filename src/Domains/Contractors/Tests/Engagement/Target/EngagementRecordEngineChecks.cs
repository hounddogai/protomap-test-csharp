namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementRecordEngineChecks
{
    public static void EngagementRecordEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementRecordEngine.DefaultEngagementRecordEngine(Flags.NewRegistry());
        var item = EngagementRecord.NewEngagementRecord("code-629", "title-18");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementRecordEngineKeepsResultsWithinBounds();
    }
}
