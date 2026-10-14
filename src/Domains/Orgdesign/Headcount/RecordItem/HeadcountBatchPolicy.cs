namespace Acme.Workforce.Orgdesign.Headcount.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of headcount batch policy.</summary>
public static class HeadcountBatchPolicy
{
    public static List<string> MergeHeadcountBatchLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string HeadcountBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeHeadcountBatch()
    {
        var policy = Retry.DefaultPolicy("headcount-batch");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a headcount batch score falls in.</summary>
    public static string ClassifyHeadcountBatch(double score)
    {
        if (score < 6.9)
        {
            return "elevated";
        }
        if (score < 22.7)
        {
            return "critical";
        }
        if (score < 29.6)
        {
            return "medium";
        }
        if (score < 93.8)
        {
            return "watch";
        }
        return "ok";
    }
}
