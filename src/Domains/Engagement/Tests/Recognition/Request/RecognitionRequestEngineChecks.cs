namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionRequestEngineChecks
{
    public static void RecognitionRequestEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionRequestEngine.DefaultRecognitionRequestEngine(Flags.NewRegistry());
        var item = RecognitionRequest.NewRecognitionRequest("code-450", "title-285");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionRequestEngineKeepsResultsWithinBounds();
    }
}
