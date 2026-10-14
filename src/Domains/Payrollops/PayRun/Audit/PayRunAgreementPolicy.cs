namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run agreement policy.</summary>
public static class PayRunAgreementPolicy
{
    /// <summary>Puts a pay run agreement code in its canonical PRA- form.</summary>
    public static string NormalizePayRunAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayRunAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.29;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunAgreement()
    {
        var policy = Retry.DefaultPolicy("pay-run-agreement");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string PayRunAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
