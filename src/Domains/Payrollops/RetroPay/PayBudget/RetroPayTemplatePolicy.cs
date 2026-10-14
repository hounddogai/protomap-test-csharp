namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay template policy.</summary>
public static class RetroPayTemplatePolicy
{
    public static double RetroPayTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.27;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string RetroPayTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a retro pay template code in its canonical RPT- form.</summary>
    public static string NormalizeRetroPayTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
