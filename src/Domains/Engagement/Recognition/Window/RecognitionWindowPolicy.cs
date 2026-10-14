namespace Acme.Workforce.Engagement.Recognition.Window;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of recognition window policy.</summary>
public static class RecognitionWindowPolicy
{
    public static string RecognitionWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRecognitionWindow()
    {
        var policy = Retry.DefaultPolicy("recognition-window");
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

    /// <summary>Names the band a recognition window score falls in.</summary>
    public static string ClassifyRecognitionWindow(double score)
    {
        if (score < 19.0)
        {
            return "elevated";
        }
        if (score < 41.8)
        {
            return "high";
        }
        return "minimal";
    }
}
