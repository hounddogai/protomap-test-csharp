namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Functions of default visitor metric policy.</summary>
public static class DefaultVisitorMetricPolicy
{
    /// <summary>Puts a default visitor metric code in its canonical DVM- form.</summary>
    public static string NormalizeDefaultVisitorMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DVM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DVM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 51672.0, in order.</summary>
    public static int DefaultVisitorMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51672.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
