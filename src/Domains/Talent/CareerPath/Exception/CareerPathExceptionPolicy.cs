namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path exception policy.</summary>
public static class CareerPathExceptionPolicy
{
    /// <summary>Puts a career path exception code in its canonical CPE- form.</summary>
    public static string NormalizeCareerPathExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeCareerPathExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 50962.0, in order.</summary>
    public static int CareerPathExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50962.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string CareerPathExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
