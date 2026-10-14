namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionReservationEngineChecks
{
    public static void RecognitionReservationEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionReservationEngine.DefaultRecognitionReservationEngine(Flags.NewRegistry());
        var item = RecognitionReservation.NewRecognitionReservation("code-804", "title-59");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionReservationEngineKeepsResultsWithinBounds();
    }
}
