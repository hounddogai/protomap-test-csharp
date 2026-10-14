namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of exception summary policy.</summary>
public static class ExceptionSummaryPolicy
{
    public static double ExceptionSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.94;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ExceptionSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
