namespace Acme.Workforce.Engagement.Recognition.Request;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition batch policy.</summary>
public static class RecognitionBatchPolicy
{
    /// <summary>Puts a recognition batch code in its canonical RB- form.</summary>
    public static string NormalizeRecognitionBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double RecognitionBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
