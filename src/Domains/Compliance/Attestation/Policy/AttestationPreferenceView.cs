namespace Acme.Workforce.Compliance.Attestation.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation preference.</summary>
public sealed record AttestationPreferenceView(string Code, string Label, string Currency, string QuantityText, string ExpiresAt, string StatusLabel)
{
    public static AttestationPreferenceView ToAttestationPreferenceView(AttestationPreference item)
    {
        return new AttestationPreferenceView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Currency,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationPreferenceView> ToAttestationPreferenceViews(List<AttestationPreference> items)
    {
        var views = new List<AttestationPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationPreferenceView(item));
        }
        return views;
    }

    public static double SumAttestationPreferenceView(AttestationPreferenceView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedExpiresAt;
    }
}
