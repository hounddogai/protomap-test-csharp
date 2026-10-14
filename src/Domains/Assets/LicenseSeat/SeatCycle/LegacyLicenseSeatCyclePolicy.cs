namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy license seat cycle policy.</summary>
public static class LegacyLicenseSeatCyclePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87148.0, in order.</summary>
    public static int LegacyLicenseSeatCycleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87148.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a legacy license seat cycle code in its canonical LLSC- form.</summary>
    public static string NormalizeLegacyLicenseSeatCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LLSC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LLSC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyLicenseSeatCycle()
    {
        var policy = Retry.DefaultPolicy("legacy-license-seat-cycle");
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

    public static string LegacyLicenseSeatCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
