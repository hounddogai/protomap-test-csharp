namespace Acme.Workforce.Workplace.Tests.AccessBadge.Request;

using Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class WeightedAccessBadgeReservationChecks
{
    public static void WeightedAccessBadgeReservationStartsInCreated()
    {
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("code-768", "title-381");
        Check.Equal(item.Status, WeightedAccessBadgeReservationStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAccessBadgeReservationMovesToPublished()
    {
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("code-768", "title-381");
        try
        {
            item.MoveTo(WeightedAccessBadgeReservationStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAccessBadgeReservationStatus.Published, "status");
    }

    public static void WeightedAccessBadgeReservationRejectsMovesFromRetired()
    {
        var item = WeightedAccessBadgeReservation.NewWeightedAccessBadgeReservation("code-768", "title-381");
        item.Status = WeightedAccessBadgeReservationStatus.Retired;
        Check.Fails(() => item.MoveTo(WeightedAccessBadgeReservationStatus.Created), "move");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeReservationStartsInCreated();
        WeightedAccessBadgeReservationMovesToPublished();
        WeightedAccessBadgeReservationRejectsMovesFromRetired();
    }
}
