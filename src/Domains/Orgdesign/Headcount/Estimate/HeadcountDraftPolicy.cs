namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of headcount draft policy.</summary>
public static class HeadcountDraftPolicy
{
    /// <summary>Names the band a headcount draft score falls in.</summary>
    public static string ClassifyHeadcountDraft(double score)
    {
        if (score < 56.3)
        {
            return "medium";
        }
        if (score < 88.1)
        {
            return "minimal";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountDraft()
    {
        var policy = Retry.DefaultPolicy("headcount-draft");
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
