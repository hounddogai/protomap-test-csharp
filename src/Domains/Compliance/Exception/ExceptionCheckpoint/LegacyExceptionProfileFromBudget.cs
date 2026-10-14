namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Certification.Review;

/// <summary>Functions of legacy exception profile from budget.</summary>
public static class LegacyExceptionProfileFromBudget
{
    public static Dictionary<string, double> SummarizeDefaultCertificationBudgetsForProfile(List<DefaultCertificationBudget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<LegacyExceptionProfile> DeriveLegacyExceptionProfilesFromBudget(List<DefaultCertificationBudget> items)
    {
        var outValue = new List<LegacyExceptionProfile>();
        foreach (var item in items)
        {
            var derived = LegacyExceptionProfile.NewLegacyExceptionProfile(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeLegacyExceptionProfile(LegacyExceptionProfile item)
    {
        return CertificationReviewPolicy.ClassifyCertificationReview(item.AdjustedTotal());
    }
}
