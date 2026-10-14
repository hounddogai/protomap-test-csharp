namespace Acme.Workforce.Assets.AssetReturn.Template;

/// <summary>Functions of manual asset return baseline policy.</summary>
public static class ManualAssetReturnBaselinePolicy
{
    public static int ManualAssetReturnBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 98286.0, in order.</summary>
    public static int ManualAssetReturnBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98286.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a manual asset return baseline code in its canonical MARB- form.</summary>
    public static string NormalizeManualAssetReturnBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MARB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MARB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
