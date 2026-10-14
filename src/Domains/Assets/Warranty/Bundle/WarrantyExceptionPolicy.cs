namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty exception policy.</summary>
public static class WarrantyExceptionPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 26859.0, in order.</summary>
    public static int WarrantyExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26859.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WarrantyExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a warranty exception score falls in.</summary>
    public static string ClassifyWarrantyException(double score)
    {
        if (score < 88.0)
        {
            return "high";
        }
        if (score < 98.8)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeWarrantyExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
