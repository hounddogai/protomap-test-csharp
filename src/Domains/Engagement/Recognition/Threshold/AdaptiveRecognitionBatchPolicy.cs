namespace Acme.Workforce.Engagement.Recognition.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive recognition batch policy.</summary>
public static class AdaptiveRecognitionBatchPolicy
{
    public static string AdaptiveRecognitionBatchBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double AdaptiveRecognitionBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
