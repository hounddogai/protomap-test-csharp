namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled nomination reservation policy.</summary>
public static class ScheduledNominationReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 85417.0, in order.</summary>
    public static int ScheduledNominationReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85417.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a scheduled nomination reservation code in its canonical SNR- form.</summary>
    public static string NormalizeScheduledNominationReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SNR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SNR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScheduledNominationReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
