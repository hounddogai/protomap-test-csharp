namespace Acme.Workforce.Engagement.Tests.Recognition.Template;

using Acme.Workforce.Engagement.Recognition.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class RollingRecognitionReservationEngineChecks
{
    public static void RollingRecognitionReservationEngineKeepsResultsWithinBounds()
    {
        var engine = RollingRecognitionReservationEngine.DefaultRollingRecognitionReservationEngine(Flags.NewRegistry());
        var item = RollingRecognitionReservation.NewRollingRecognitionReservation("code-385", "title-361");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingRecognitionReservationEngineKeepsResultsWithinBounds();
    }
}
