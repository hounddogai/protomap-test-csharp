namespace Acme.Workforce.Compensation.BonusPool.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of bonus pool agreement policy.</summary>
public static class BonusPoolAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBonusPoolAgreement()
    {
        var policy = Retry.DefaultPolicy("bonus-pool-agreement");
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

    /// <summary>Puts a bonus pool agreement code in its canonical BPA- form.</summary>
    public static string NormalizeBonusPoolAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string BonusPoolAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a bonus pool agreement score falls in.</summary>
    public static string ClassifyBonusPoolAgreement(double score)
    {
        if (score < 19.4)
        {
            return "elevated";
        }
        if (score < 31.6)
        {
            return "high";
        }
        if (score < 68.2)
        {
            return "watch";
        }
        return "minimal";
    }
}
