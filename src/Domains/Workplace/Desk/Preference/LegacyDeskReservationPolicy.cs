namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy desk reservation policy.</summary>
public static class LegacyDeskReservationPolicy
{
    /// <summary>Puts a legacy desk reservation code in its canonical LDR- form.</summary>
    public static string NormalizeLegacyDeskReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LDR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LDR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLegacyDeskReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LegacyDeskReservationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 58087.0, in order.</summary>
    public static int LegacyDeskReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58087.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
