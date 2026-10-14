namespace Acme.Workforce.Engagement.Recognition.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition baseline policy.</summary>
public static class RecognitionBaselinePolicy
{
    public static string RecognitionBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 67896.0, in order.</summary>
    public static int RecognitionBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67896.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a recognition baseline code in its canonical RB- form.</summary>
    public static string NormalizeRecognitionBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
