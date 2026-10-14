namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor allocation policy.</summary>
public static class VisitorAllocationPolicy
{
    public static double VisitorAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.15;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 20588.0, in order.</summary>
    public static int VisitorAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20588.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a visitor allocation code in its canonical VA- form.</summary>
    public static string NormalizeVisitorAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string VisitorAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
