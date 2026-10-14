namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledBookingTemplateEngineChecks
{
    public static void ScheduledBookingTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBookingTemplateEngine.DefaultScheduledBookingTemplateEngine(Flags.NewRegistry());
        var item = ScheduledBookingTemplate.NewScheduledBookingTemplate("code-651", "name-222");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBookingTemplateEngineKeepsResultsWithinBounds();
    }
}
