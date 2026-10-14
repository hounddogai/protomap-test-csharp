namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global assignment reservation policy.</summary>
public static class GlobalAssignmentReservationPolicy
{
    /// <summary>Names the band a global assignment reservation score falls in.</summary>
    public static string ClassifyGlobalAssignmentReservation(double score)
    {
        if (score < 25.3)
        {
            return "ok";
        }
        if (score < 33.5)
        {
            return "watch";
        }
        if (score < 50.5)
        {
            return "minimal";
        }
        return "medium";
    }

    public static double GlobalAssignmentReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.25;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
