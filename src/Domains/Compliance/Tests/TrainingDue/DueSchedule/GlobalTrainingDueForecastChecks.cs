namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class GlobalTrainingDueForecastChecks
{
    public static void GlobalTrainingDueForecastStartsInProposed()
    {
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("code-715", "title-847");
        Check.Equal(item.Status, GlobalTrainingDueForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalTrainingDueForecastMovesToApproved()
    {
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("code-715", "title-847");
        try
        {
            item.MoveTo(GlobalTrainingDueForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalTrainingDueForecastStatus.Approved, "status");
    }

    public static void GlobalTrainingDueForecastRejectsMovesFromWithdrawn()
    {
        var item = GlobalTrainingDueForecast.NewGlobalTrainingDueForecast("code-715", "title-847");
        item.Status = GlobalTrainingDueForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(GlobalTrainingDueForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        GlobalTrainingDueForecastStartsInProposed();
        GlobalTrainingDueForecastMovesToApproved();
        GlobalTrainingDueForecastRejectsMovesFromWithdrawn();
    }
}
