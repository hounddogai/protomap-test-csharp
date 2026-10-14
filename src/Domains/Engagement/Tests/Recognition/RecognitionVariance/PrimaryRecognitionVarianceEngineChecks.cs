namespace Acme.Workforce.Engagement.Tests.Recognition.RecognitionVariance;

using Acme.Workforce.Engagement.Recognition.RecognitionVariance;

/// <summary>Checks of recognition variance code, run by the project's runner.</summary>
public static class PrimaryRecognitionVarianceEngineChecks
{
    public static void PrimaryRecognitionVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryRecognitionVarianceEngine.DefaultPrimaryRecognitionVarianceEngine();
        var item = PrimaryRecognitionVariance.NewPrimaryRecognitionVariance("code-807", "title-942");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryRecognitionVarianceEngineKeepsResultsWithinBounds();
    }
}
