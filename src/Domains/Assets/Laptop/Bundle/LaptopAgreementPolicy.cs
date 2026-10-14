namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of laptop agreement policy.</summary>
public static class LaptopAgreementPolicy
{
    public static string LaptopAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 65419.0, in order.</summary>
    public static int LaptopAgreementBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65419.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLaptopAgreement()
    {
        var policy = Retry.DefaultPolicy("laptop-agreement");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a laptop agreement code in its canonical LA- form.</summary>
    public static string NormalizeLaptopAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
