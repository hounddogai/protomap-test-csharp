namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification plan policy.</summary>
public static class CertificationPlanPolicy
{
    public static double CertificationPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string CertificationPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int CertificationPlanSpan(int start, int end)
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
}
