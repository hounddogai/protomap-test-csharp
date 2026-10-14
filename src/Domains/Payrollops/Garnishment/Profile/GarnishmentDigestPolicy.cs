namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of garnishment digest policy.</summary>
public static class GarnishmentDigestPolicy
{
    public static List<string> MergeGarnishmentDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a garnishment digest score falls in.</summary>
    public static string ClassifyGarnishmentDigest(double score)
    {
        if (score < 40.4)
        {
            return "low";
        }
        if (score < 66.5)
        {
            return "critical";
        }
        if (score < 80.2)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 36374.0, in order.</summary>
    public static int GarnishmentDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36374.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
