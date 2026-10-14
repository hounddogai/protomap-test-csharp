namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default statement of work mapping policy.</summary>
public static class DefaultStatementOfWorkMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 75837.0, in order.</summary>
    public static int DefaultStatementOfWorkMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 75837.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string DefaultStatementOfWorkMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double DefaultStatementOfWorkMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.97;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a default statement of work mapping code in its canonical DSOWM- form.</summary>
    public static string NormalizeDefaultStatementOfWorkMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DSOWM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DSOWM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
