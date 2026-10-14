namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a fallback attestation agreement.</summary>
public sealed record FallbackAttestationAgreementView(string Code, string Name, string Segment, string Score, string ExpiresAt, string Description, string StatusLabel)
{
    public static FallbackAttestationAgreementView ToFallbackAttestationAgreementView(FallbackAttestationAgreement item)
    {
        return new FallbackAttestationAgreementView(
            item.Code,
            item.Name,
            item.Segment,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Description,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAttestationAgreementView> ToFallbackAttestationAgreementViews(List<FallbackAttestationAgreement> items)
    {
        var views = new List<FallbackAttestationAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAttestationAgreementView(item));
        }
        return views;
    }

    public static double SumFallbackAttestationAgreementView(FallbackAttestationAgreementView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedExpiresAt;
    }
}
