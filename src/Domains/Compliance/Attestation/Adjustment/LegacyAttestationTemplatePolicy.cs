namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy attestation template policy.</summary>
public static class LegacyAttestationTemplatePolicy
{
    /// <summary>Names the band a legacy attestation template score falls in.</summary>
    public static string ClassifyLegacyAttestationTemplate(double score)
    {
        if (score < 8.0)
        {
            return "elevated";
        }
        if (score < 55.0)
        {
            return "critical";
        }
        if (score < 57.7)
        {
            return "ok";
        }
        if (score < 99.7)
        {
            return "high";
        }
        return "low";
    }

    public static List<string> MergeLegacyAttestationTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 38896.0, in order.</summary>
    public static int LegacyAttestationTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 38896.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int LegacyAttestationTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
