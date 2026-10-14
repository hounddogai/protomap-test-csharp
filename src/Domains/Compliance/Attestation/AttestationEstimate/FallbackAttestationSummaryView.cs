namespace Acme.Workforce.Compliance.Attestation.AttestationEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback attestation summary.</summary>
public sealed record FallbackAttestationSummaryView(string Code, string Name, string Checksum, string SourceText, string TeamText, string Quantity, string StatusLabel)
{
    public static FallbackAttestationSummaryView ToFallbackAttestationSummaryView(FallbackAttestationSummary item)
    {
        return new FallbackAttestationSummaryView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Checksum, 40),
            Text.Truncate(item.Source, 120),
            item.Team,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAttestationSummaryView> ToFallbackAttestationSummaryViews(List<FallbackAttestationSummary> items)
    {
        var views = new List<FallbackAttestationSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAttestationSummaryView(item));
        }
        return views;
    }

    public static double SumFallbackAttestationSummaryView(FallbackAttestationSummaryView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
