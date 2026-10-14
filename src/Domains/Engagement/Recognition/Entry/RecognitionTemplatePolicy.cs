namespace Acme.Workforce.Engagement.Recognition.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of recognition template policy.</summary>
public static class RecognitionTemplatePolicy
{
    public static int RecognitionTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double RecognitionTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.51;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a recognition template score falls in.</summary>
    public static string ClassifyRecognitionTemplate(double score)
    {
        if (score < 73.2)
        {
            return "minimal";
        }
        if (score < 77.7)
        {
            return "ok";
        }
        return "elevated";
    }
}
