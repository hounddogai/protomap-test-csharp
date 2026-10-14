namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Compliance.Certification.CertificationForecast;

/// <summary>Functions of certification review from forecast.</summary>
public static class CertificationReviewFromForecast
{
    public static Dictionary<string, double> SummarizeManualCertificationForecastsForReview(List<ManualCertificationForecast> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<CertificationReview> DeriveCertificationReviewsFromForecast(List<ManualCertificationForecast> items)
    {
        var outValue = new List<CertificationReview>();
        foreach (var item in items)
        {
            var derived = CertificationReview.NewCertificationReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeCertificationReview(CertificationReview item)
    {
        return CappedCertificationTemplatePolicy.NormalizeCappedCertificationTemplateCode(item.Code);
    }
}
