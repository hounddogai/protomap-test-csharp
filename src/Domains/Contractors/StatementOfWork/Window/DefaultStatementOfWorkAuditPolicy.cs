namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default statement of work audit policy.</summary>
public static class DefaultStatementOfWorkAuditPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 36427.0, in order.</summary>
    public static int DefaultStatementOfWorkAuditBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36427.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double DefaultStatementOfWorkAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a default statement of work audit code in its canonical DSOWA- form.</summary>
    public static string NormalizeDefaultStatementOfWorkAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DSOWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DSOWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
