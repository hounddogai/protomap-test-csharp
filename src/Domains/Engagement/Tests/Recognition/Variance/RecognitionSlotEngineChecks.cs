namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionSlotEngineChecks
{
    public static void RecognitionSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionSlotEngine.DefaultRecognitionSlotEngine();
        var item = RecognitionSlot.NewRecognitionSlot("code-310", "name-761");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionSlotEngineKeepsResultsWithinBounds();
    }
}
