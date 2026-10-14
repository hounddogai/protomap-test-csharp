namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled training due audit policy.</summary>
public static class ScheduledTrainingDueAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 35969.0, in order.</summary>
    public static int ScheduledTrainingDueAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 35969.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string ScheduledTrainingDueAuditBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a scheduled training due audit code in its canonical STDA- form.</summary>
    public static string NormalizeScheduledTrainingDueAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STDA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STDA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int ScheduledTrainingDueAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
