namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional training due request policy.</summary>
public static class RegionalTrainingDueRequestPolicy
{
    public static string RegionalTrainingDueRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 41191.0, in order.</summary>
    public static int RegionalTrainingDueRequestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41191.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a regional training due request code in its canonical RTDR- form.</summary>
    public static string NormalizeRegionalTrainingDueRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RTDR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RTDR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalTrainingDueRequestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
