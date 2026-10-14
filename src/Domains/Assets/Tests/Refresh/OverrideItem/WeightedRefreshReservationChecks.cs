namespace Acme.Workforce.Assets.Tests.Refresh.OverrideItem;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedRefreshReservationChecks
{
    public static void WeightedRefreshReservationStartsInDraft()
    {
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("code-10", "label-791");
        Check.Equal(item.Status, WeightedRefreshReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRefreshReservationMovesToPending()
    {
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("code-10", "label-791");
        try
        {
            item.MoveTo(WeightedRefreshReservationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRefreshReservationStatus.Pending, "status");
    }

    public static void WeightedRefreshReservationRejectsMovesFromClosed()
    {
        var item = WeightedRefreshReservation.NewWeightedRefreshReservation("code-10", "label-791");
        item.Status = WeightedRefreshReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedRefreshReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedRefreshReservationStartsInDraft();
        WeightedRefreshReservationMovesToPending();
        WeightedRefreshReservationRejectsMovesFromClosed();
    }
}
