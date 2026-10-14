namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of warranty agreement policy.</summary>
public static class WarrantyAgreementPolicy
{
    /// <summary>Names the band a warranty agreement score falls in.</summary>
    public static string ClassifyWarrantyAgreement(double score)
    {
        if (score < 30.7)
        {
            return "low";
        }
        if (score < 32.7)
        {
            return "medium";
        }
        if (score < 99.1)
        {
            return "watch";
        }
        return "minimal";
    }

    public static List<string> MergeWarrantyAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
