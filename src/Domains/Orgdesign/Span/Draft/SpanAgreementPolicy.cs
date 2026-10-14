namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of span agreement policy.</summary>
public static class SpanAgreementPolicy
{
    public static int SpanAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double SpanAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
