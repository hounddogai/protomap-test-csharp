namespace Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Functions of adaptive career path notice policy.</summary>
public static class AdaptiveCareerPathNoticePolicy
{
    /// <summary>Puts a adaptive career path notice code in its canonical ACPN- form.</summary>
    public static string NormalizeAdaptiveCareerPathNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ACPN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ACPN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 69244.0, in order.</summary>
    public static int AdaptiveCareerPathNoticeBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 69244.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
