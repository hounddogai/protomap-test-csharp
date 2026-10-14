namespace Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Functions of retro pay review policy.</summary>
public static class RetroPayReviewPolicy
{
    /// <summary>Puts a retro pay review code in its canonical RPR- form.</summary>
    public static string NormalizeRetroPayReviewCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 51870.0, in order.</summary>
    public static int RetroPayReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51870.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
