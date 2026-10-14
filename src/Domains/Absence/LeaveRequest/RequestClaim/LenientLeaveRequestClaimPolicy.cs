namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient leave request claim policy.</summary>
public static class LenientLeaveRequestClaimPolicy
{
    public static string LenientLeaveRequestClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a lenient leave request claim score falls in.</summary>
    public static string ClassifyLenientLeaveRequestClaim(double score)
    {
        if (score < 1.1)
        {
            return "watch";
        }
        if (score < 59.7)
        {
            return "ok";
        }
        if (score < 89.0)
        {
            return "critical";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLenientLeaveRequestClaim()
    {
        var policy = Retry.DefaultPolicy("lenient-leave-request-claim");
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

    public static List<string> MergeLenientLeaveRequestClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
