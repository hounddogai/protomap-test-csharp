namespace Acme.Workforce.Assets.LicenseSeat.SeatReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of license seat window policy.</summary>
public static class LicenseSeatWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 82726.0, in order.</summary>
    public static int LicenseSeatWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82726.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeLicenseSeatWindowLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a license seat window code in its canonical LSW- form.</summary>
    public static string NormalizeLicenseSeatWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
