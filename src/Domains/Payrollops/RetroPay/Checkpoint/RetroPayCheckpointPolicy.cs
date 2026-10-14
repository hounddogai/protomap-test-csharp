namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Functions of retro pay checkpoint policy.</summary>
public static class RetroPayCheckpointPolicy
{
    /// <summary>Puts a retro pay checkpoint code in its canonical RPC- form.</summary>
    public static string NormalizeRetroPayCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 89148.0, in order.</summary>
    public static int RetroPayCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 89148.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
