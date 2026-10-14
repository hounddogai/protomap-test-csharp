namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient leave request agreement policy.</summary>
public static class LenientLeaveRequestAgreementPolicy
{
    /// <summary>Names the band a lenient leave request agreement score falls in.</summary>
    public static string ClassifyLenientLeaveRequestAgreement(double score)
    {
        if (score < 14.9)
        {
            return "critical";
        }
        if (score < 23.3)
        {
            return "low";
        }
        return "elevated";
    }

    public static string LenientLeaveRequestAgreementBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double LenientLeaveRequestAgreementWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeLenientLeaveRequestAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
