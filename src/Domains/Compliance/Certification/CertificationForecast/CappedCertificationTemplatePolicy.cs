namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of capped certification template policy.</summary>
public static class CappedCertificationTemplatePolicy
{
    public static string CappedCertificationTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 92931.0, in order.</summary>
    public static int CappedCertificationTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 92931.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a capped certification template code in its canonical CCT- form.</summary>
    public static string NormalizeCappedCertificationTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CCT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CCT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int CappedCertificationTemplateSpan(int start, int end)
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
