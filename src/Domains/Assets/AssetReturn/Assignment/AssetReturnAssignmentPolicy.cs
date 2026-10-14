namespace Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Functions of asset return assignment policy.</summary>
public static class AssetReturnAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 59464.0, in order.</summary>
    public static int AssetReturnAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 59464.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a asset return assignment code in its canonical ARA- form.</summary>
    public static string NormalizeAssetReturnAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AssetReturnAssignmentSpan(int start, int end)
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
}
