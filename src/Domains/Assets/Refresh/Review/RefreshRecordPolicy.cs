namespace Acme.Workforce.Assets.Refresh.Review;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh record policy.</summary>
public static class RefreshRecordPolicy
{
    public static List<string> MergeRefreshRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRefreshRecord()
    {
        var policy = Retry.DefaultPolicy("refresh-record");
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

    /// <summary>Names the band a refresh record score falls in.</summary>
    public static string ClassifyRefreshRecord(double score)
    {
        if (score < 68.7)
        {
            return "low";
        }
        if (score < 80.2)
        {
            return "watch";
        }
        return "minimal";
    }

    public static string RefreshRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
