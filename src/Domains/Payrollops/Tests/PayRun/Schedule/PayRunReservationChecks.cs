namespace Acme.Workforce.Payrollops.Tests.PayRun.Schedule;

using Acme.Workforce.Payrollops.PayRun.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PayRunReservationChecks
{
    public static void PayRunReservationStartsInNew()
    {
        var item = PayRunReservation.NewPayRunReservation("code-419", "title-768");
        Check.Equal(item.Status, PayRunReservationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunReservationMovesToFailed()
    {
        var item = PayRunReservation.NewPayRunReservation("code-419", "title-768");
        try
        {
            item.MoveTo(PayRunReservationStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunReservationStatus.Failed, "status");
    }

    public static void PayRunReservationRejectsMovesFromFailed()
    {
        var item = PayRunReservation.NewPayRunReservation("code-419", "title-768");
        item.Status = PayRunReservationStatus.Failed;
        Check.Fails(() => item.MoveTo(PayRunReservationStatus.New), "move");
    }

    public static void RunAll()
    {
        PayRunReservationStartsInNew();
        PayRunReservationMovesToFailed();
        PayRunReservationRejectsMovesFromFailed();
    }
}
