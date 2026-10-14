namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LenientRecognitionEntryEngineChecks
{
    public static void LenientRecognitionEntryEngineKeepsResultsWithinBounds()
    {
        var engine = LenientRecognitionEntryEngine.DefaultLenientRecognitionEntryEngine(Flags.NewRegistry());
        var item = LenientRecognitionEntry.NewLenientRecognitionEntry("code-814", "title-357");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientRecognitionEntryEngineKeepsResultsWithinBounds();
    }
}
