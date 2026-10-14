namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of license seat forecast policy.</summary>
public static class LicenseSeatForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 92857.0, in order.</summary>
    public static int LicenseSeatForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92857.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLicenseSeatForecast()
    {
        var policy = Retry.DefaultPolicy("license-seat-forecast");
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
}
