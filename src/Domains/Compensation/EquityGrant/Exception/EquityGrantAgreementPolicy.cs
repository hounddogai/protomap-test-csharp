namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant agreement policy.</summary>
public static class EquityGrantAgreementPolicy
{
    /// <summary>Names the band a equity grant agreement score falls in.</summary>
    public static string ClassifyEquityGrantAgreement(double score)
    {
        if (score < 40.5)
        {
            return "ok";
        }
        if (score < 72.5)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 33819.0, in order.</summary>
    public static int EquityGrantAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33819.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeEquityGrantAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
