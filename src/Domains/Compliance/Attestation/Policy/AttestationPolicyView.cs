namespace Acme.Workforce.Compliance.Attestation.Policy;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation policy.</summary>
public sealed record AttestationPolicyView(string Code, string Name, string MarginText, string Sequence, string Visible, string SalaryBand, string Title, string DiscountText, string Unit, string Label, string StatusLabel)
{
    public static AttestationPolicyView ToAttestationPolicyView(AttestationPolicy item)
    {
        return new AttestationPolicyView(
            item.Code,
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.SalaryBand, 80),
            item.Title,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 80),
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationPolicyView> ToAttestationPolicyViews(List<AttestationPolicy> items)
    {
        var views = new List<AttestationPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationPolicyView(item));
        }
        return views;
    }

    public static double SumAttestationPolicyView(AttestationPolicyView view)
    {
        var parsedMargin = double.Parse(view.MarginText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedSequence + parsedDiscount;
    }
}
