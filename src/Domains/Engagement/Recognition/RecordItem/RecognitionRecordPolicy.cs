namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of recognition record policy.</summary>
public static class RecognitionRecordPolicy
{
    /// <summary>Puts a recognition record code in its canonical RR- form.</summary>
    public static string NormalizeRecognitionRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionRecord()
    {
        var policy = Retry.DefaultPolicy("recognition-record");
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

    /// <summary>Names the band a recognition record score falls in.</summary>
    public static string ClassifyRecognitionRecord(double score)
    {
        if (score < 17.4)
        {
            return "ok";
        }
        if (score < 59.8)
        {
            return "watch";
        }
        if (score < 77.5)
        {
            return "minimal";
        }
        return "high";
    }

    public static List<string> MergeRecognitionRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
