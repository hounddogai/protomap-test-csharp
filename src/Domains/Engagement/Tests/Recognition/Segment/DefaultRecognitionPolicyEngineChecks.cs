namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class DefaultRecognitionPolicyEngineChecks
{
    public static void DefaultRecognitionPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultRecognitionPolicyEngine.DefaultDefaultRecognitionPolicyEngine(Flags.NewRegistry());
        var item = DefaultRecognitionPolicy.NewDefaultRecognitionPolicy("code-947", "name-928");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultRecognitionPolicyEngineKeepsResultsWithinBounds();
    }
}
