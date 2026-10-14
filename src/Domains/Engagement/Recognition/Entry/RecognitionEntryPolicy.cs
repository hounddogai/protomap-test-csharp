namespace Acme.Workforce.Engagement.Recognition.Entry;

/// <summary>Functions of recognition entry policy.</summary>
public static class RecognitionEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 40874.0, in order.</summary>
    public static int RecognitionEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 40874.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int RecognitionEntrySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a recognition entry code in its canonical RE- form.</summary>
    public static string NormalizeRecognitionEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
