namespace Acme.Workforce.Assets.Laptop.Milestone;

/// <summary>Functions of laptop milestone policy.</summary>
public static class LaptopMilestonePolicy
{
    public static int LaptopMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Puts a laptop milestone code in its canonical LM- form.</summary>
    public static string NormalizeLaptopMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
