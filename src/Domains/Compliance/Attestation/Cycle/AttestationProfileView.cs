namespace Acme.Workforce.Compliance.Attestation.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation profile.</summary>
public sealed record AttestationProfileView(string CodeText, string Label, string Country, string Quantity, string Archived, string Currency, string Summary, string StatusLabel)
{
    public static AttestationProfileView ToAttestationProfileView(AttestationProfile item)
    {
        return new AttestationProfileView(
            Text.Truncate(item.Code, 40),
            item.Label,
            Text.Truncate(item.Country, 64),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            Text.Truncate(item.Currency, 120),
            Text.Truncate(item.Summary, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationProfileView> ToAttestationProfileViews(List<AttestationProfile> items)
    {
        var views = new List<AttestationProfileView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationProfileView(item));
        }
        return views;
    }

    public static double SumAttestationProfileView(AttestationProfileView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
