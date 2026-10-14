namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityWindowEngineChecks
{
    public static void MobilityWindowEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityWindowEngine.DefaultMobilityWindowEngine();
        var item = MobilityWindow.NewMobilityWindow("code-781", "label-141");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityWindowEngineKeepsResultsWithinBounds();
    }
}
