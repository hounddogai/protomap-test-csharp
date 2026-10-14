namespace Acme.Workforce.Compliance.Attestation.Template;

/// <summary>Functions of default attestation template policy.</summary>
public static class DefaultAttestationTemplatePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99636.0, in order.</summary>
    public static int DefaultAttestationTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99636.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a default attestation template score falls in.</summary>
    public static string ClassifyDefaultAttestationTemplate(double score)
    {
        if (score < 4.7)
        {
            return "low";
        }
        if (score < 63.1)
        {
            return "critical";
        }
        if (score < 73.0)
        {
            return "ok";
        }
        if (score < 92.0)
        {
            return "elevated";
        }
        return "watch";
    }

    public static int DefaultAttestationTemplateSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }
}
