namespace Acme.Workforce.Compliance.Certification.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of certification schedule policy.</summary>
public static class CertificationSchedulePolicy
{
    /// <summary>Puts a certification schedule code in its canonical CS- form.</summary>
    public static string NormalizeCertificationScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CertificationScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static string CertificationScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationSchedule()
    {
        var policy = Retry.DefaultPolicy("certification-schedule");
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
}
