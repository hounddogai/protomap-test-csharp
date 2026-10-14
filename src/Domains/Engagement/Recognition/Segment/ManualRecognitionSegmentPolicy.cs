namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual recognition segment policy.</summary>
public static class ManualRecognitionSegmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualRecognitionSegment()
    {
        var policy = Retry.DefaultPolicy("manual-recognition-segment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a manual recognition segment score falls in.</summary>
    public static string ClassifyManualRecognitionSegment(double score)
    {
        if (score < 5.7)
        {
            return "elevated";
        }
        if (score < 14.3)
        {
            return "high";
        }
        if (score < 66.0)
        {
            return "minimal";
        }
        if (score < 68.2)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 22363.0, in order.</summary>
    public static int ManualRecognitionSegmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 22363.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double ManualRecognitionSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
