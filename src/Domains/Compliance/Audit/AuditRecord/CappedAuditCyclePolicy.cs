namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped audit cycle policy.</summary>
public static class CappedAuditCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 58753.0, in order.</summary>
    public static int CappedAuditCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58753.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a capped audit cycle code in its canonical CAC- form.</summary>
    public static string NormalizeCappedAuditCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CAC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CAC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double CappedAuditCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string CappedAuditCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
