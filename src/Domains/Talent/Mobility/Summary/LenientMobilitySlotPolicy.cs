namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient mobility slot policy.</summary>
public static class LenientMobilitySlotPolicy
{
    public static double LenientMobilitySlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 969.0, in order.</summary>
    public static int LenientMobilitySlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 969.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a lenient mobility slot code in its canonical LMS- form.</summary>
    public static string NormalizeLenientMobilitySlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LMS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LMS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
