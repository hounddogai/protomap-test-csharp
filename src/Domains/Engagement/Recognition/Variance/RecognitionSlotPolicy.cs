namespace Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Functions of recognition slot policy.</summary>
public static class RecognitionSlotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 12210.0, in order.</summary>
    public static int RecognitionSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 12210.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a recognition slot code in its canonical RS- form.</summary>
    public static string NormalizeRecognitionSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
