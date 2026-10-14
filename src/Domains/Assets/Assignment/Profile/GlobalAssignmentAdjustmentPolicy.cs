namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global assignment adjustment policy.</summary>
public static class GlobalAssignmentAdjustmentPolicy
{
    public static string GlobalAssignmentAdjustmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a global assignment adjustment code in its canonical GAA- form.</summary>
    public static string NormalizeGlobalAssignmentAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
