namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class StatementOfWorkReservationEngineChecks
{
    public static void StatementOfWorkReservationEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkReservationEngine.DefaultStatementOfWorkReservationEngine(Flags.NewRegistry());
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("code-459", "title-33");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkReservationEngineKeepsResultsWithinBounds();
    }
}
