namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict attestation estimate.</summary>
public sealed record StrictAttestationEstimateView(string CodeText, string NameText, string Badge, string SalaryBand, string Approved, string LocationText, string LimitText, string StatusLabel)
{
    public static StrictAttestationEstimateView ToStrictAttestationEstimateView(StrictAttestationEstimate item)
    {
        return new StrictAttestationEstimateView(
            item.Code,
            item.Name,
            Text.Truncate(item.Badge, 64),
            item.SalaryBand,
            $"{item.Approved}",
            item.Location,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAttestationEstimateView> ToStrictAttestationEstimateViews(List<StrictAttestationEstimate> items)
    {
        var views = new List<StrictAttestationEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAttestationEstimateView(item));
        }
        return views;
    }

    public static double SumStrictAttestationEstimateView(StrictAttestationEstimateView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedLimit;
    }
}
