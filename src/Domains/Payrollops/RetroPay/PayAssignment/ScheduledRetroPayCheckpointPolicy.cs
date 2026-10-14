namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled retro pay checkpoint policy.</summary>
public static class ScheduledRetroPayCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 25498.0, in order.</summary>
    public static int ScheduledRetroPayCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25498.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeScheduledRetroPayCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a scheduled retro pay checkpoint code in its canonical SRPC- form.</summary>
    public static string NormalizeScheduledRetroPayCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
