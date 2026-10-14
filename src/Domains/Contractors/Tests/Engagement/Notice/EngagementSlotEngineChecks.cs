namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementSlotEngineChecks
{
    public static void EngagementSlotEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementSlotEngine.DefaultEngagementSlotEngine(Flags.NewRegistry());
        var item = EngagementSlot.NewEngagementSlot("code-225", "label-764");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementSlotEngineKeepsResultsWithinBounds();
    }
}
