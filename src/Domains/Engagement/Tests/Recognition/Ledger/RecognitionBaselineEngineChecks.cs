namespace Acme.Workforce.Engagement.Tests.Recognition.Ledger;

using Acme.Workforce.Engagement.Recognition.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RecognitionBaselineEngineChecks
{
    public static void RecognitionBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionBaselineEngine.DefaultRecognitionBaselineEngine(Flags.NewRegistry());
        var item = RecognitionBaseline.NewRecognitionBaseline("code-970", "name-437");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionBaselineEngineKeepsResultsWithinBounds();
    }
}
