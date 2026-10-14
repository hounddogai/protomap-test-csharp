namespace Acme.Workforce.Workplace.Desk.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary desk forecast policy.</summary>
public static class PrimaryDeskForecastPolicy
{
    /// <summary>Names the band a primary desk forecast score falls in.</summary>
    public static string ClassifyPrimaryDeskForecast(double score)
    {
        if (score < 25.6)
        {
            return "elevated";
        }
        if (score < 29.5)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 24082.0, in order.</summary>
    public static int PrimaryDeskForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 24082.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergePrimaryDeskForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
