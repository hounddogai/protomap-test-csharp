namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of span template policy.</summary>
public static class SpanTemplatePolicy
{
    /// <summary>Names the band a span template score falls in.</summary>
    public static string ClassifySpanTemplate(double score)
    {
        if (score < 5.5)
        {
            return "elevated";
        }
        if (score < 14.7)
        {
            return "high";
        }
        if (score < 94.8)
        {
            return "low";
        }
        return "minimal";
    }

    public static int SpanTemplateSpan(int start, int end)
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

    public static string SpanTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
