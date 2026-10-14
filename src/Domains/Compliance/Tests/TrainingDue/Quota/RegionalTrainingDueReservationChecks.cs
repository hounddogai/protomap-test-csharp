namespace Acme.Workforce.Compliance.Tests.TrainingDue.Quota;

using Acme.Workforce.Compliance.TrainingDue.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class RegionalTrainingDueReservationChecks
{
    public static void RegionalTrainingDueReservationStartsInNew()
    {
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("code-868", "name-936");
        Check.Equal(item.Status, RegionalTrainingDueReservationStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTrainingDueReservationMovesToRunning()
    {
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("code-868", "name-936");
        try
        {
            item.MoveTo(RegionalTrainingDueReservationStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTrainingDueReservationStatus.Running, "status");
    }

    public static void RegionalTrainingDueReservationRejectsMovesFromFailed()
    {
        var item = RegionalTrainingDueReservation.NewRegionalTrainingDueReservation("code-868", "name-936");
        item.Status = RegionalTrainingDueReservationStatus.Failed;
        Check.Fails(() => item.MoveTo(RegionalTrainingDueReservationStatus.New), "move");
    }

    public static void RunAll()
    {
        RegionalTrainingDueReservationStartsInNew();
        RegionalTrainingDueReservationMovesToRunning();
        RegionalTrainingDueReservationRejectsMovesFromFailed();
    }
}
