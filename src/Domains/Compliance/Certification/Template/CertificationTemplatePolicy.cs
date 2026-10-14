namespace Acme.Workforce.Compliance.Certification.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of certification template policy.</summary>
public static class CertificationTemplatePolicy
{
    public static string CertificationTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 63108.0, in order.</summary>
    public static int CertificationTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63108.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a certification template score falls in.</summary>
    public static string ClassifyCertificationTemplate(double score)
    {
        if (score < 52.9)
        {
            return "low";
        }
        if (score < 57.8)
        {
            return "high";
        }
        if (score < 63.1)
        {
            return "elevated";
        }
        return "medium";
    }
}
