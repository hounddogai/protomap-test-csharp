namespace Acme.Workforce.Compliance.Attestation.Adjustment;

using System.Globalization;

/// <summary>How screens and exports show a primary attestation policy.</summary>
public sealed record PrimaryAttestationPolicyView(string Code, string NameText, string DiscountText, string Badge, string Limit, string StatusLabel)
{
    public static PrimaryAttestationPolicyView ToPrimaryAttestationPolicyView(PrimaryAttestationPolicy item)
    {
        return new PrimaryAttestationPolicyView(
            item.Code,
            item.Name,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryAttestationPolicyView> ToPrimaryAttestationPolicyViews(List<PrimaryAttestationPolicy> items)
    {
        var views = new List<PrimaryAttestationPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryAttestationPolicyView(item));
        }
        return views;
    }

    public static double SumPrimaryAttestationPolicyView(PrimaryAttestationPolicyView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedDiscount + (double)parsedLimit;
    }
}
