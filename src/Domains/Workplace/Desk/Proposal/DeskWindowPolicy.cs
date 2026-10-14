namespace Acme.Workforce.Workplace.Desk.Proposal;

/// <summary>Functions of desk window policy.</summary>
public static class DeskWindowPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 67298.0, in order.</summary>
    public static int DeskWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 67298.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int DeskWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a desk window code in its canonical DW- form.</summary>
    public static string NormalizeDeskWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a desk window score falls in.</summary>
    public static string ClassifyDeskWindow(double score)
    {
        if (score < 39.4)
        {
            return "critical";
        }
        if (score < 54.1)
        {
            return "elevated";
        }
        if (score < 60.8)
        {
            return "minimal";
        }
        if (score < 97.8)
        {
            return "watch";
        }
        return "high";
    }
}
