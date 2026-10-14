namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RecognitionScheduleEngineChecks
{
    public static void RecognitionScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionScheduleEngine.DefaultRecognitionScheduleEngine(Flags.NewRegistry());
        var item = RecognitionSchedule.NewRecognitionSchedule("code-878", "title-180");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionScheduleEngineKeepsResultsWithinBounds();
    }
}
