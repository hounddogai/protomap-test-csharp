namespace Acme.Workforce.Assets.Laptop.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy laptop template policy.</summary>
public static class LegacyLaptopTemplatePolicy
{
    /// <summary>Puts a legacy laptop template code in its canonical LLT- form.</summary>
    public static string NormalizeLegacyLaptopTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LegacyLaptopTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeLegacyLaptopTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
