namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk slot policy.</summary>
public static class DeskSlotPolicy
{
    /// <summary>Puts a desk slot code in its canonical DS- form.</summary>
    public static string NormalizeDeskSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double DeskSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
