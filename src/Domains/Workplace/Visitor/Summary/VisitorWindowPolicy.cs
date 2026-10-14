namespace Acme.Workforce.Workplace.Visitor.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor window policy.</summary>
public static class VisitorWindowPolicy
{
    public static double VisitorWindowWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string VisitorWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
