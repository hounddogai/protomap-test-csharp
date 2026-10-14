namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of manual mobility record policy.</summary>
public static class ManualMobilityRecordPolicy
{
    /// <summary>Puts a manual mobility record code in its canonical MMR- form.</summary>
    public static string NormalizeManualMobilityRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MMR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MMR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 13089.0, in order.</summary>
    public static int ManualMobilityRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 13089.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int ManualMobilityRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static double ManualMobilityRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
