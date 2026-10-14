namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped retro pay forecast policy.</summary>
public static class CappedRetroPayForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 20768.0, in order.</summary>
    public static int CappedRetroPayForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 20768.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedRetroPayForecast()
    {
        var policy = Retry.DefaultPolicy("capped-retro-pay-forecast");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a capped retro pay forecast code in its canonical CRPF- form.</summary>
    public static string NormalizeCappedRetroPayForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CRPF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CRPF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
