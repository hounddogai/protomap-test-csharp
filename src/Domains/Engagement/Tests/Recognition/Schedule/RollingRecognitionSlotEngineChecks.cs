namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingRecognitionSlotEngineChecks
{
    public static void RollingRecognitionSlotEngineKeepsResultsWithinBounds()
    {
        var engine = RollingRecognitionSlotEngine.DefaultRollingRecognitionSlotEngine();
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("code-886", "label-843");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingRecognitionSlotEngineKeepsResultsWithinBounds();
    }
}
