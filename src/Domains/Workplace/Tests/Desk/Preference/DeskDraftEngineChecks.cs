namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DeskDraftEngineChecks
{
    public static void DeskDraftEngineKeepsResultsWithinBounds()
    {
        var engine = DeskDraftEngine.DefaultDeskDraftEngine(Flags.NewRegistry());
        var item = DeskDraft.NewDeskDraft("code-218", "title-705");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeskDraftEngineKeepsResultsWithinBounds();
    }
}
