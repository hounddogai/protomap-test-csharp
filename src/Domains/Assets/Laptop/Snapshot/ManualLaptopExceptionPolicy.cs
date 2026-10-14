namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual laptop exception policy.</summary>
public static class ManualLaptopExceptionPolicy
{
    /// <summary>Names the band a manual laptop exception score falls in.</summary>
    public static string ClassifyManualLaptopException(double score)
    {
        if (score < 1.2)
        {
            return "critical";
        }
        if (score < 16.7)
        {
            return "minimal";
        }
        if (score < 53.0)
        {
            return "elevated";
        }
        if (score < 70.3)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Puts a manual laptop exception code in its canonical MLE- form.</summary>
    public static string NormalizeManualLaptopExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MLE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MLE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ManualLaptopExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
