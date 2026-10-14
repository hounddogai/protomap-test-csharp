namespace Acme.Workforce.Payrollops.RetroPay.Proposal;

/// <summary>Functions of retro pay reservation policy.</summary>
public static class RetroPayReservationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92100.0, in order.</summary>
    public static int RetroPayReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92100.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a retro pay reservation score falls in.</summary>
    public static string ClassifyRetroPayReservation(double score)
    {
        if (score < 14.1)
        {
            return "elevated";
        }
        if (score < 95.5)
        {
            return "medium";
        }
        return "ok";
    }

    /// <summary>Puts a retro pay reservation code in its canonical RPR- form.</summary>
    public static string NormalizeRetroPayReservationCode(string code)
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
}
