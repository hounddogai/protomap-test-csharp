namespace Acme.Workforce.Engagement.Tests.Recognition.Agreement;

using Acme.Workforce.Engagement.Recognition.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RecognitionAgreementEngineChecks
{
    public static void RecognitionAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionAgreementEngine.DefaultRecognitionAgreementEngine(Flags.NewRegistry());
        var item = RecognitionAgreement.NewRecognitionAgreement("code-833", "label-865");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionAgreementEngineKeepsResultsWithinBounds();
    }
}
