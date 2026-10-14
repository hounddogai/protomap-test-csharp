namespace Acme.Workforce.Compliance.Tests.PolicyAck.Reservation;

using Acme.Workforce.Compliance.PolicyAck.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class PolicyAckReservationEngineChecks
{
    public static void PolicyAckReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckReservationEngine.DefaultPolicyAckReservationEngine();
        var item = PolicyAckReservation.NewPolicyAckReservation("code-930", "title-430");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckReservationEngineKeepsResultsWithinBounds();
    }
}
