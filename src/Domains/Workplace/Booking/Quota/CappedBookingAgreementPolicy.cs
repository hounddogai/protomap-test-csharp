namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped booking agreement policy.</summary>
public static class CappedBookingAgreementPolicy
{
    public static List<string> MergeCappedBookingAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CappedBookingAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedBookingAgreement()
    {
        var policy = Retry.DefaultPolicy("capped-booking-agreement");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a capped booking agreement score falls in.</summary>
    public static string ClassifyCappedBookingAgreement(double score)
    {
        if (score < 53.4)
        {
            return "low";
        }
        if (score < 87.4)
        {
            return "minimal";
        }
        if (score < 89.0)
        {
            return "critical";
        }
        return "medium";
    }
}
