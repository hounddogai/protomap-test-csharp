namespace Acme.Workforce.Contractors.Engagement.Manifest;

/// <summary>Functions of engagement reservation policy.</summary>
public static class EngagementReservationPolicy
{
    /// <summary>Names the band a engagement reservation score falls in.</summary>
    public static string ClassifyEngagementReservation(double score)
    {
        if (score < 21.6)
        {
            return "elevated";
        }
        if (score < 44.3)
        {
            return "high";
        }
        if (score < 75.1)
        {
            return "critical";
        }
        if (score < 95.5)
        {
            return "minimal";
        }
        return "medium";
    }

    public static int EngagementReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }
}
