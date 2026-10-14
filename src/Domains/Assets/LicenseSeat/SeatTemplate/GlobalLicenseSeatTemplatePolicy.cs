namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global license seat template policy.</summary>
public static class GlobalLicenseSeatTemplatePolicy
{
    /// <summary>Puts a global license seat template code in its canonical GLST- form.</summary>
    public static string NormalizeGlobalLicenseSeatTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GLST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GLST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GlobalLicenseSeatTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
