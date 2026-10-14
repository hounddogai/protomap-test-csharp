namespace Acme.Workforce.Talent.Mobility.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of mobility template policy.</summary>
public static class MobilityTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeMobilityTemplate()
    {
        var policy = Retry.DefaultPolicy("mobility-template");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int MobilityTemplateSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a mobility template score falls in.</summary>
    public static string ClassifyMobilityTemplate(double score)
    {
        if (score < 28.1)
        {
            return "watch";
        }
        if (score < 100.0)
        {
            return "high";
        }
        return "medium";
    }
}
