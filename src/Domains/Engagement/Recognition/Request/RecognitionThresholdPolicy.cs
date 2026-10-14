namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition threshold policy.</summary>
public static class RecognitionThresholdPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 66357.0, in order.</summary>
    public static int RecognitionThresholdBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 66357.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RecognitionThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a recognition threshold code in its canonical RT- form.</summary>
    public static string NormalizeRecognitionThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
