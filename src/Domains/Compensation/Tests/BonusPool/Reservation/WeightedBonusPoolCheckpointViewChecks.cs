namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedBonusPoolCheckpointViewChecks
{
    public static void WeightedBonusPoolCheckpointViewShowsTheStatusInCapitals()
    {
        var item = WeightedBonusPoolCheckpoint.NewWeightedBonusPoolCheckpoint("code-238", "label-674");
        var view = WeightedBonusPoolCheckpointView.ToWeightedBonusPoolCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBonusPoolCheckpointView.ToWeightedBonusPoolCheckpointViews(new List<WeightedBonusPoolCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBonusPoolCheckpointViewShowsTheStatusInCapitals();
    }
}
