namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of warranty template policy.</summary>
public static class WarrantyTemplatePolicy
{
    /// <summary>Puts a warranty template code in its canonical WT- form.</summary>
    public static string NormalizeWarrantyTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string WarrantyTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
