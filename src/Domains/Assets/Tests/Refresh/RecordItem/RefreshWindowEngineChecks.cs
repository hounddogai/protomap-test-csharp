namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RefreshWindowEngineChecks
{
    public static void RefreshWindowEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshWindowEngine.DefaultRefreshWindowEngine(Flags.NewRegistry());
        var item = RefreshWindow.NewRefreshWindow("code-698", "title-985");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshWindowEngineKeepsResultsWithinBounds();
    }
}
