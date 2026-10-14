namespace Acme.Workforce.Payrollops.Tests.RetroPay.Proposal;

using Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RetroPayReservationChecks
{
    public static void RetroPayReservationStartsInIdle()
    {
        var item = RetroPayReservation.NewRetroPayReservation("code-116", "label-44");
        Check.Equal(item.Status, RetroPayReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayReservationMovesToReady()
    {
        var item = RetroPayReservation.NewRetroPayReservation("code-116", "label-44");
        try
        {
            item.MoveTo(RetroPayReservationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayReservationStatus.Ready, "status");
    }

    public static void RetroPayReservationRejectsMovesFromStopped()
    {
        var item = RetroPayReservation.NewRetroPayReservation("code-116", "label-44");
        item.Status = RetroPayReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(RetroPayReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RetroPayReservationStartsInIdle();
        RetroPayReservationMovesToReady();
        RetroPayReservationRejectsMovesFromStopped();
    }
}
