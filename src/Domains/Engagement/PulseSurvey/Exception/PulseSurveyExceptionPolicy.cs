namespace Acme.Workforce.Engagement.PulseSurvey.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pulse survey exception policy.</summary>
public static class PulseSurveyExceptionPolicy
{
    public static double PulseSurveyExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a pulse survey exception code in its canonical PSE- form.</summary>
    public static string NormalizePulseSurveyExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PSE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PSE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
