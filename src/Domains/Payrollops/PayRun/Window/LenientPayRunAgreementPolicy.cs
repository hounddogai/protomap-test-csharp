namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient pay run agreement policy.</summary>
public static class LenientPayRunAgreementPolicy
{
    /// <summary>Names the band a lenient pay run agreement score falls in.</summary>
    public static string ClassifyLenientPayRunAgreement(double score)
    {
        if (score < 30.2)
        {
            return "critical";
        }
        if (score < 35.2)
        {
            return "high";
        }
        if (score < 52.6)
        {
            return "ok";
        }
        if (score < 82.9)
        {
            return "watch";
        }
        return "low";
    }

    public static List<string> MergeLenientPayRunAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 49579.0, in order.</summary>
    public static int LenientPayRunAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 49579.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientPayRunAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
