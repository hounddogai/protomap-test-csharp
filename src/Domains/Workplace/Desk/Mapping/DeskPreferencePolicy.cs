namespace Acme.Workforce.Workplace.Desk.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of desk preference policy.</summary>
public static class DeskPreferencePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31228.0, in order.</summary>
    public static int DeskPreferenceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31228.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a desk preference code in its canonical DP- form.</summary>
    public static string NormalizeDeskPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string DeskPreferenceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
