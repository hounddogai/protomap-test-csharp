namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary recognition policy policy.</summary>
public static class PrimaryRecognitionPolicyPolicy
{
    public static int PrimaryRecognitionPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryRecognitionPolicy()
    {
        var policy = Retry.DefaultPolicy("primary-recognition-policy");
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
