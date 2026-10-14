namespace Acme.Workforce.Payrollops.PayRun.Schedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pay run reservation policy.</summary>
public static class PayRunReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 45806.0, in order.</summary>
    public static int PayRunReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 45806.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePayRunReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a pay run reservation score falls in.</summary>
    public static string ClassifyPayRunReservation(double score)
    {
        if (score < 24.3)
        {
            return "low";
        }
        if (score < 30.9)
        {
            return "medium";
        }
        if (score < 39.7)
        {
            return "minimal";
        }
        return "critical";
    }

    public static double PayRunReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
