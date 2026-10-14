namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of license seat assignment policy.</summary>
public static class LicenseSeatAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72295.0, in order.</summary>
    public static int LicenseSeatAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72295.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a license seat assignment code in its canonical LSA- form.</summary>
    public static string NormalizeLicenseSeatAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeLicenseSeatAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
