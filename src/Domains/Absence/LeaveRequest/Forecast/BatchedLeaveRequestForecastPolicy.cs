namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched leave request forecast policy.</summary>
public static class BatchedLeaveRequestForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 69606.0, in order.</summary>
    public static int BatchedLeaveRequestForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69606.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a batched leave request forecast score falls in.</summary>
    public static string ClassifyBatchedLeaveRequestForecast(double score)
    {
        if (score < 5.0)
        {
            return "ok";
        }
        if (score < 87.8)
        {
            return "critical";
        }
        if (score < 99.2)
        {
            return "medium";
        }
        return "low";
    }

    public static List<string> MergeBatchedLeaveRequestForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double BatchedLeaveRequestForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
