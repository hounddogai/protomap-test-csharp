namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling asset return forecast policy.</summary>
public static class RollingAssetReturnForecastPolicy
{
    /// <summary>Puts a rolling asset return forecast code in its canonical RARF- form.</summary>
    public static string NormalizeRollingAssetReturnForecastCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RARF-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RARF-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 61278.0, in order.</summary>
    public static int RollingAssetReturnForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 61278.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingAssetReturnForecast()
    {
        var policy = Retry.DefaultPolicy("rolling-asset-return-forecast");
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
