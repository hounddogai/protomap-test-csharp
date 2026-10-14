namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient position audit policy.</summary>
public static class LenientPositionAuditPolicy
{
    public static double LenientPositionAuditWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.82;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int LenientPositionAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
