namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict recognition policy policy.</summary>
public static class StrictRecognitionPolicyPolicy
{
    /// <summary>Puts a strict recognition policy code in its canonical SRP- form.</summary>
    public static string NormalizeStrictRecognitionPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 289.0, in order.</summary>
    public static int StrictRecognitionPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 289.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string StrictRecognitionPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
