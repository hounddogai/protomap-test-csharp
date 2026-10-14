namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of engagement template policy.</summary>
public static class EngagementTemplatePolicy
{
    public static string EngagementTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double EngagementTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
