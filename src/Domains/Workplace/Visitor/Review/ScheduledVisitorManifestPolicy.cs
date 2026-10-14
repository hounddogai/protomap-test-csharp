namespace Acme.Workforce.Workplace.Visitor.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled visitor manifest policy.</summary>
public static class ScheduledVisitorManifestPolicy
{
    public static string ScheduledVisitorManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a scheduled visitor manifest score falls in.</summary>
    public static string ClassifyScheduledVisitorManifest(double score)
    {
        if (score < 3.5)
        {
            return "minimal";
        }
        if (score < 36.2)
        {
            return "critical";
        }
        if (score < 73.4)
        {
            return "elevated";
        }
        if (score < 78.2)
        {
            return "watch";
        }
        return "high";
    }

    /// <summary>Puts a scheduled visitor manifest code in its canonical SVM- form.</summary>
    public static string NormalizeScheduledVisitorManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SVM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SVM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 27514.0, in order.</summary>
    public static int ScheduledVisitorManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 27514.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
