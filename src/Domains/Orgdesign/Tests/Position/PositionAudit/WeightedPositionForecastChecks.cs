namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class WeightedPositionForecastChecks
{
    public static void WeightedPositionForecastStartsInProposed()
    {
        var item = WeightedPositionForecast.NewWeightedPositionForecast("code-219", "label-757");
        Check.Equal(item.Status, WeightedPositionForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedPositionForecastMovesToRejected()
    {
        var item = WeightedPositionForecast.NewWeightedPositionForecast("code-219", "label-757");
        try
        {
            item.MoveTo(WeightedPositionForecastStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedPositionForecastStatus.Rejected, "status");
    }

    public static void WeightedPositionForecastRejectsMovesFromWithdrawn()
    {
        var item = WeightedPositionForecast.NewWeightedPositionForecast("code-219", "label-757");
        item.Status = WeightedPositionForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedPositionForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedPositionForecastStartsInProposed();
        WeightedPositionForecastMovesToRejected();
        WeightedPositionForecastRejectsMovesFromWithdrawn();
    }
}
