namespace Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Functions of retro pay adjustment policy.</summary>
public static class RetroPayAdjustmentPolicy
{
    /// <summary>Puts a retro pay adjustment code in its canonical RPA- form.</summary>
    public static string NormalizeRetroPayAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a retro pay adjustment score falls in.</summary>
    public static string ClassifyRetroPayAdjustment(double score)
    {
        if (score < 28.6)
        {
            return "high";
        }
        if (score < 47.0)
        {
            return "ok";
        }
        if (score < 74.8)
        {
            return "low";
        }
        if (score < 96.7)
        {
            return "critical";
        }
        return "minimal";
    }

    public static int RetroPayAdjustmentSpan(int start, int end)
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

    /// <summary>Counts how many amounts fit in a budget of 29697.0, in order.</summary>
    public static int RetroPayAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 29697.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
