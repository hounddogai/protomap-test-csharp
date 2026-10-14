namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict pay band draft policy.</summary>
public static class StrictPayBandDraftPolicy
{
    /// <summary>Names the band a strict pay band draft score falls in.</summary>
    public static string ClassifyStrictPayBandDraft(double score)
    {
        if (score < 48.9)
        {
            return "low";
        }
        if (score < 77.2)
        {
            return "minimal";
        }
        return "medium";
    }

    public static List<string> MergeStrictPayBandDraftLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPayBandDraft()
    {
        var policy = Retry.DefaultPolicy("strict-pay-band-draft");
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
}
