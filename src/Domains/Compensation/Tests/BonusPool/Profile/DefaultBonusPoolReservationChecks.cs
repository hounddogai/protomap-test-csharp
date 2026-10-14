namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultBonusPoolReservationChecks
{
    public static void DefaultBonusPoolReservationStartsInPlanned()
    {
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("code-458", "name-11");
        Check.Equal(item.Status, DefaultBonusPoolReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultBonusPoolReservationMovesToScheduled()
    {
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("code-458", "name-11");
        try
        {
            item.MoveTo(DefaultBonusPoolReservationStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultBonusPoolReservationStatus.Scheduled, "status");
    }

    public static void DefaultBonusPoolReservationRejectsMovesFromCancelled()
    {
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("code-458", "name-11");
        item.Status = DefaultBonusPoolReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultBonusPoolReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultBonusPoolReservationStartsInPlanned();
        DefaultBonusPoolReservationMovesToScheduled();
        DefaultBonusPoolReservationRejectsMovesFromCancelled();
    }
}
