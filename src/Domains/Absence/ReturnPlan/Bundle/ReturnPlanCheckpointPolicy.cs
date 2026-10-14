namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of return plan checkpoint policy.</summary>
public static class ReturnPlanCheckpointPolicy
{
    /// <summary>Puts a return plan checkpoint code in its canonical RPC- form.</summary>
    public static string NormalizeReturnPlanCheckpointCode(string code)
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

    public static string ReturnPlanCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 32158.0, in order.</summary>
    public static int ReturnPlanCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 32158.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
