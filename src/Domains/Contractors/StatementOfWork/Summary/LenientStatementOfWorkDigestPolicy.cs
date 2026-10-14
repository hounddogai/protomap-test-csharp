namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient statement of work digest policy.</summary>
public static class LenientStatementOfWorkDigestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 15827.0, in order.</summary>
    public static int LenientStatementOfWorkDigestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 15827.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double LenientStatementOfWorkDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.22;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string LenientStatementOfWorkDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a lenient statement of work digest code in its canonical LSOWD- form.</summary>
    public static string NormalizeLenientStatementOfWorkDigestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSOWD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSOWD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
