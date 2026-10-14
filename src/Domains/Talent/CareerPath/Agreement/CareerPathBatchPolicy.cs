namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of career path batch policy.</summary>
public static class CareerPathBatchPolicy
{
    public static int CareerPathBatchSpan(int start, int end)
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

    /// <summary>Puts a career path batch code in its canonical CPB- form.</summary>
    public static string NormalizeCareerPathBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CareerPathBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 40734.0, in order.</summary>
    public static int CareerPathBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40734.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
