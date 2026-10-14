namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of span forecast policy.</summary>
public static class SpanForecastPolicy
{
    public static int SpanForecastSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a span forecast score falls in.</summary>
    public static string ClassifySpanForecast(double score)
    {
        if (score < 15.3)
        {
            return "minimal";
        }
        if (score < 20.4)
        {
            return "ok";
        }
        if (score < 48.7)
        {
            return "elevated";
        }
        if (score < 49.4)
        {
            return "medium";
        }
        return "watch";
    }

    public static double SpanForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
