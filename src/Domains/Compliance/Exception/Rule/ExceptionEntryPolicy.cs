namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception entry policy.</summary>
public static class ExceptionEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36366.0, in order.</summary>
    public static int ExceptionEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36366.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ExceptionEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a exception entry code in its canonical EE- form.</summary>
    public static string NormalizeExceptionEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
