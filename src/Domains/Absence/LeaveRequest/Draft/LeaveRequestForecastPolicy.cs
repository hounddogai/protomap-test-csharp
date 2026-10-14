namespace Acme.Workforce.Absence.LeaveRequest.Draft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of leave request forecast policy.</summary>
public static class LeaveRequestForecastPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLeaveRequestForecast()
    {
        var policy = Retry.DefaultPolicy("leave-request-forecast");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double LeaveRequestForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a leave request forecast code in its canonical LRF- form.</summary>
    public static string NormalizeLeaveRequestForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a leave request forecast score falls in.</summary>
    public static string ClassifyLeaveRequestForecast(double score)
    {
        if (score < 50.4)
        {
            return "minimal";
        }
        if (score < 67.5)
        {
            return "ok";
        }
        if (score < 73.0)
        {
            return "medium";
        }
        if (score < 86.4)
        {
            return "watch";
        }
        return "elevated";
    }
}
