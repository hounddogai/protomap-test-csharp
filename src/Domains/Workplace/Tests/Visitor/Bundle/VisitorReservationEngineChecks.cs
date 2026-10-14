namespace Acme.Workforce.Workplace.Tests.Visitor.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class VisitorReservationEngineChecks
{
    public static void VisitorReservationEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorReservationEngine.DefaultVisitorReservationEngine(Flags.NewRegistry());
        var item = VisitorReservation.NewVisitorReservation("code-720", "title-944");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorReservationEngineKeepsResultsWithinBounds();
    }
}
