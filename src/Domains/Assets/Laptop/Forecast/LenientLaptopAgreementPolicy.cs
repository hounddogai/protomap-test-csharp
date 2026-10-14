namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient laptop agreement policy.</summary>
public static class LenientLaptopAgreementPolicy
{
    public static List<string> MergeLenientLaptopAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double LenientLaptopAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient laptop agreement score falls in.</summary>
    public static string ClassifyLenientLaptopAgreement(double score)
    {
        if (score < 53.8)
        {
            return "ok";
        }
        if (score < 74.2)
        {
            return "watch";
        }
        if (score < 89.8)
        {
            return "high";
        }
        return "critical";
    }
}
