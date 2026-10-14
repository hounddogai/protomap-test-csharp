namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of occupancy template policy.</summary>
public static class OccupancyTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 40065.0, in order.</summary>
    public static int OccupancyTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40065.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a occupancy template score falls in.</summary>
    public static string ClassifyOccupancyTemplate(double score)
    {
        if (score < 34.4)
        {
            return "elevated";
        }
        if (score < 52.8)
        {
            return "minimal";
        }
        if (score < 54.4)
        {
            return "watch";
        }
        if (score < 60.6)
        {
            return "ok";
        }
        return "medium";
    }

    public static List<string> MergeOccupancyTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
