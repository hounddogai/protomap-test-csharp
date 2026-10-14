namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback attestation claim.</summary>
public sealed record FallbackAttestationClaimView(string Code, string Label, string FloorValueText, string RetriesText, string Quantity, string Amount, string ExternalRef, string CostCenterText, string MaximumText, string StatusLabel)
{
    public static FallbackAttestationClaimView ToFallbackAttestationClaimView(FallbackAttestationClaim item)
    {
        return new FallbackAttestationClaimView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            Text.Truncate(item.CostCenter, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAttestationClaimView> ToFallbackAttestationClaimViews(List<FallbackAttestationClaim> items)
    {
        var views = new List<FallbackAttestationClaimView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAttestationClaimView(item));
        }
        return views;
    }

    public static double SumFallbackAttestationClaimView(FallbackAttestationClaimView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedRetries + (double)parsedQuantity;
    }
}
