namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition snapshot policy.</summary>
public static class RecognitionSnapshotPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 69198.0, in order.</summary>
    public static int RecognitionSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69198.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string RecognitionSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a recognition snapshot code in its canonical RS- form.</summary>
    public static string NormalizeRecognitionSnapshotCode(string code)
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
