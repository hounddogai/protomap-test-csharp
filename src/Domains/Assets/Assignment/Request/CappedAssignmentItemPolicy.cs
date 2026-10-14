namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped assignment item policy.</summary>
public static class CappedAssignmentItemPolicy
{
    /// <summary>Puts a capped assignment item code in its canonical CAI- form.</summary>
    public static string NormalizeCappedAssignmentItemCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CAI-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CAI-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double CappedAssignmentItemWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.26;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int CappedAssignmentItemSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Counts how many amounts fit in a budget of 6289.0, in order.</summary>
    public static int CappedAssignmentItemBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 6289.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
