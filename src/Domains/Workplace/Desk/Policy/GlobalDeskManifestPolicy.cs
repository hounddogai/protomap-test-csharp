namespace Acme.Workforce.Workplace.Desk.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global desk manifest policy.</summary>
public static class GlobalDeskManifestPolicy
{
    public static string GlobalDeskManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a global desk manifest code in its canonical GDM- form.</summary>
    public static string NormalizeGlobalDeskManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GDM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GDM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
