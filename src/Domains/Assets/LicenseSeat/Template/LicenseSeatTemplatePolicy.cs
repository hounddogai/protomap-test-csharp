namespace Acme.Workforce.Assets.LicenseSeat.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat template policy.</summary>
public static class LicenseSeatTemplatePolicy
{
    public static double LicenseSeatTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a license seat template score falls in.</summary>
    public static string ClassifyLicenseSeatTemplate(double score)
    {
        if (score < 8.4)
        {
            return "high";
        }
        if (score < 55.7)
        {
            return "watch";
        }
        if (score < 70.4)
        {
            return "medium";
        }
        if (score < 90.9)
        {
            return "low";
        }
        return "ok";
    }

    public static string LicenseSeatTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
