namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Functions of statement of work reservation policy.</summary>
public static class StatementOfWorkReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 41995.0, in order.</summary>
    public static int StatementOfWorkReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 41995.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a statement of work reservation code in its canonical SOWR- form.</summary>
    public static string NormalizeStatementOfWorkReservationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SOWR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SOWR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int StatementOfWorkReservationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
