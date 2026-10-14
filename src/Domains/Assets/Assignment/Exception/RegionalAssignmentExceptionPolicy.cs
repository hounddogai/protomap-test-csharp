namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional assignment exception policy.</summary>
public static class RegionalAssignmentExceptionPolicy
{
    public static double RegionalAssignmentExceptionWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RegionalAssignmentExceptionSpan(int start, int end)
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
        return span / 3600;
    }
}
