namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class StatementOfWorkReservationChecks
{
    public static void StatementOfWorkReservationStartsInRequested()
    {
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("code-459", "title-33");
        Check.Equal(item.Status, StatementOfWorkReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkReservationMovesToGranted()
    {
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("code-459", "title-33");
        try
        {
            item.MoveTo(StatementOfWorkReservationStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkReservationStatus.Granted, "status");
    }

    public static void StatementOfWorkReservationRejectsMovesFromExpired()
    {
        var item = StatementOfWorkReservation.NewStatementOfWorkReservation("code-459", "title-33");
        item.Status = StatementOfWorkReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(StatementOfWorkReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkReservationStartsInRequested();
        StatementOfWorkReservationMovesToGranted();
        StatementOfWorkReservationRejectsMovesFromExpired();
    }
}
