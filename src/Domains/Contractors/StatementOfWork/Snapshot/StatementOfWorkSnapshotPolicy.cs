namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of statement of work snapshot policy.</summary>
public static class StatementOfWorkSnapshotPolicy
{
    /// <summary>Puts a statement of work snapshot code in its canonical SOWS- form.</summary>
    public static string NormalizeStatementOfWorkSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StatementOfWorkSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 81242.0, in order.</summary>
    public static int StatementOfWorkSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 81242.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string StatementOfWorkSnapshotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
